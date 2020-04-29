using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Rogue;

public class Player : Singleton<Player>, IPointerDownHandler, IPointerUpHandler 
{
    public float speed;
    public int stepCount;

    Vector2 point;
    public Vector3 stepPoint;
   
    public bool isAnimation = false;
    public bool isMoving = false;
    bool isDeath = false;

    public Sprite[] sprites = new Sprite[4];
    private Camera cam;
    Animation anim;

    public AudioClip PunchSound;
    public AudioClip getDamage;

    public void OnPointerDown(PointerEventData eventData) //вызывается когда мышь нажата 
    { 

    } 
    public void OnPointerUp(PointerEventData eventData) //вызывается когда мышь отпущена
    {   
        point = cam.ScreenToWorldPoint(new Vector3((int)Mathf.Round(eventData.position.x), (int)Mathf.Round(eventData.position.y), 0)); //из локальных координат в мировые
        point = new Vector2((int)Mathf.Round(point.x), (int)Mathf.Round(point.y));
        isMoving = true; //запуск движения`
    }

    void Start()
    {
        anim = gameObject.GetComponent<Animation>();
        cam = Camera.main;
        stepPoint = transform.position;
    }

    void Update()
    {   
        if (isMoving && !isDeath)
        {
            Move();  
        }
    }

    void Move()
    {
        float step = speed * Time.deltaTime;

        if(transform.position == stepPoint && !isAnimation)
        {
            for(int i = 0; i < Generator.Instance.enemies.Length; i++)
            {
                Enemy enemy = Generator.Instance.enemies[i].GetComponent<Enemy>();
                enemy.isPunch = true;
            }
            (stepPoint.x,stepPoint.y) = FindWave((int)transform.position.x, (int)transform.position.y, (int)point.x, (int)point.y); 
        }

        if(Generator.Instance.tiles[(int)stepPoint.x][(int)stepPoint.y] == Generator.TileType.Enemy)
        {
            HitEnemy((int)stepPoint.x, (int)stepPoint.y);
            stepPoint = transform.position;
            isMoving = false;
        }
        else if(Generator.Instance.tiles[(int)stepPoint.x][(int)stepPoint.y] == Generator.TileType.Wall || Generator.Instance.tiles[(int)stepPoint.x][(int)stepPoint.y] == Generator.TileType.Object)
        {        
            stepPoint = transform.position;
            isMoving = false;
        }

        else if (transform.position.x == (int)point.x && transform.position.y == (int)point.y)
            isMoving = false; 

        else
        {
            ChangeSprite();
            transform.position = Vector2.MoveTowards(transform.position, stepPoint, step);  
            cam.transform.position = new Vector3 (transform.position.x, transform.position.y, -5);
  
        }  
    }

    void HitEnemy(int x, int y)
    {
        for(int i = 0; i < Generator.Instance.enemies.Length; i++)
        {
            Enemy enemy = Generator.Instance.enemies[i].GetComponent<Enemy>();
            enemy.isPunch = true;
            enemy.isStep = true;
            if(x == Generator.Instance.enemies[i].transform.position.x && y == Generator.Instance.enemies[i].transform.position.y)
            {
                AudioManager.Instance.PlayEffects(PunchSound);
                ChangeSprite();
                enemy.getDamage(1);
            }
        }
    }

    public void GetDamage(int l)
    {
        anim.Play("GetDamage");
        GameManager.Instance.PlayerLifes -= l;
        AudioManager.Instance.PlayEffects(getDamage);
        if(GameManager.Instance.PlayerLifes <= 0)
        {
            anim.Play("Death");
            GameManager.Instance.GameOver();
            isDeath = true;
        }
    }

    (int a, int b) FindPlace(int x, int y, int sx, int sy)
    {
        if((sx == x - 1 && sy == y) || (sx == x + 1 && sy == y) || (sx == x && sy == y - 1) || (sx == x && sy == y + 1))
            return (x,y);
        if((Generator.Instance.tiles[x+1][y]==Generator.TileType.Floor || Generator.Instance.tiles[x+1][y]==Generator.TileType.CorridorFloor) && sx>x)
            return (x+1,y);
        if((Generator.Instance.tiles[x-1][y]==Generator.TileType.Floor || Generator.Instance.tiles[x-1][y]==Generator.TileType.CorridorFloor) && sx<x)
            return (x-1,y);
        if((Generator.Instance.tiles[x][y+1]==Generator.TileType.Floor || Generator.Instance.tiles[x][y+1]==Generator.TileType.CorridorFloor) && sy>y)
            return (x,y+1);
        if((Generator.Instance.tiles[x][y-1]==Generator.TileType.Floor || Generator.Instance.tiles[x][y-1]==Generator.TileType.CorridorFloor) && sy<y)
            return (x,y-1);
        if(Generator.Instance.tiles[x+1][y]==Generator.TileType.Floor || Generator.Instance.tiles[x+1][y]==Generator.TileType.CorridorFloor)
            return (x+1,y);
        if(Generator.Instance.tiles[x-1][y]==Generator.TileType.Floor || Generator.Instance.tiles[x-1][y]==Generator.TileType.CorridorFloor)
            return (x-1,y);
        if(Generator.Instance.tiles[x][y+1]==Generator.TileType.Floor || Generator.Instance.tiles[x][y+1]==Generator.TileType.CorridorFloor)
            return (x,y+1);
        if(Generator.Instance.tiles[x][y-1]==Generator.TileType.Floor || Generator.Instance.tiles[x][y-1]==Generator.TileType.CorridorFloor)
            return (x,y-1);
        return((int)transform.position.x, (int)transform.position.y);
    }

    void startAnimation()
    {
        isAnimation = true;
    }

    void endAnimation()
    {
        isAnimation = false;
    }

    void ChangeSprite()
    {
        if(stepPoint.x > (int)transform.position.x)
            GetComponent<SpriteRenderer>().sprite = sprites[0];
        else if(stepPoint.x < (int)transform.position.x)
            GetComponent<SpriteRenderer>().sprite = sprites[1];
        else if(stepPoint.y > (int)transform.position.y)
            GetComponent<SpriteRenderer>().sprite = sprites[2];
        else if(stepPoint.y < (int)transform.position.y)
            GetComponent<SpriteRenderer>().sprite = sprites[3];
    }

    (int a, int b) FindWave(int startX, int startY, int targetX, int targetY) //Волновой алгоритм
    {
        int x, y,step=0;
        int stepX = 0, stepY = 0;
        int[,] cMap = new int[Generator.Instance.MapColumns, Generator.Instance.MapRows];

        for (x = 0; x < Generator.Instance.MapColumns; x++) //заполнение массива числами
            for (y = 0; y < Generator.Instance.MapRows; y++)
            {
                if (Generator.Instance.tiles[x][y] != Generator.TileType.Floor && 
                    Generator.Instance.tiles[x][y] != Generator.TileType.CorridorFloor && 
                    Generator.Instance.tiles[x][y] != Generator.TileType.End)
                    cMap[x, y] = -2; //есть препятствие
                else
                    cMap[x, y] = -1; //путь свободен
            }

        
        if(startX == targetX && startY == targetY)
        {
            isMoving = false;
            return (startX, startY);
        }

        cMap[targetX,targetY]=0; //отсчет начинается с конечной точки

        while (true) //поиск пути
        {
            for (x = startX - 8; x < startX + 8; x++)
                for (y = startY - 8; y < startY + 8; y++)
                {
                    if (cMap[x, y] == step)
                    {
                        if (x - 1 >= 0)
                            if (cMap[x - 1, y] == -1)
                                cMap[x - 1, y] = step + 1;
                        
                            if (y - 1 >= 0)
                            if (cMap[x, y - 1] == -1)
                                cMap[x, y - 1] = step + 1;
                        
                            if (x + 1 < Generator.Instance.MapColumns)
                            if (cMap[x + 1, y] == -1)
                                cMap[x + 1, y] = step + 1;

                            if (y + 1 < Generator.Instance.MapRows)
                            if (cMap[x, y + 1] == -1)
                                cMap[x, y + 1] = step + 1;
                    }
                }
            step++;
            if (cMap[startX, startY] != -1) //удалось найти путь
                break;
            if (step > 20 * 20){ //если путь не удалось найти = возвращает стартовую точку
                isMoving = false;
                return (startX, startY);
            }
        }

        x = startX; 
        y = startY;
        step = int.MaxValue;

        if (x - 1 >= 0)
            if (cMap[x - 1, y] >= 0 && cMap[x - 1, y] < step)
            {
                step = cMap[x - 1, y];
                stepX = x - 1;
                stepY = y;
                return (stepX,stepY);
            }    
        if (y - 1 >= 0)
            if (cMap[x, y - 1] >= 0 && cMap[x, y - 1] < step)
            {
                step = cMap[x, y - 1];
                stepX = x;
                stepY = y - 1;
                return (stepX,stepY);
            }      
        if (x + 1 < Generator.Instance.MapRows)
            if (cMap[x + 1, y] < step && cMap[x + 1, y] >= 0)
            {
                step = cMap[x + 1, y];
                stepX = x + 1;
                stepY = y;
                return (stepX,stepY);
            }                
        if (y + 1 < Generator.Instance.MapColumns )
            if (cMap[x, y + 1] < step && cMap[x, y + 1] >= 0)
            {
                step = cMap[x, y + 1];
                stepX = x;
                stepY = y + 1;
                return (stepX,stepY);
            }
            
        isMoving = false;
        return (startX,startY);
    }
}

