using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
{
    public GameObject layoutRoom;
    public Color startColor, endColor, shopColor, grColor;

    public int distanceToEnd;
    public bool includeShop;
    public int minDistanceToShop, maxDistanceToShop;
    public bool includeGunRoom;
    public int minDistanceToGunRoom, maxDistanceToGunRoom;

    public Transform generatorPoint;
    //enum = como um array, pode armazenar diferentes números e dar um nome pra eles "enum is just a way of giving labels to values"
    public enum Direction { up, right, down, left};
    public Direction selectedDirection;
    //Essa é a quantidade de unidade que o gerador deve se mover para criar uma sala nova, com posições x e y.
    public float xOffSet = 18f, yOffSet = 10;

    public LayerMask whatIsRoom;

    private GameObject endRoom, shopRoom, gunRoom;

    private List<GameObject> layoutRoomObjects = new List<GameObject>();

    public RoomPrefabs rooms;

    private List<GameObject> generatedOutlines = new List<GameObject>();

    //Essas variáveis são para definir o centro das salas do começo e fim, já as salas normais vão ser aleatórias dentro de um array
    public RoomCenter centerStart, centerEnd, centerShop, centerGunRoom;
    public RoomCenter[] potentialCenters;


    void Start()
    {
        Instantiate(layoutRoom, generatorPoint.position, generatorPoint.rotation).GetComponent<SpriteRenderer>().color = startColor;
        //(Direction) é para converter a variável selectedDirection em int. 
        selectedDirection = (Direction)Random.Range(0, 4);
        MoveGenerationPoint();
        //Loop de for que irá se repetir até que o número spawnado de salas seja = distanceToEnd (dintancia para acabar o spawn)
        for(int i = 0; i < distanceToEnd; i++)
        {
            GameObject newRoom = Instantiate(layoutRoom, generatorPoint.position, generatorPoint.rotation);

            layoutRoomObjects.Add(newRoom);

            //i + 1 == distance End faz referência à última sala instanciada
            if (i + 1 == distanceToEnd)
            {
                newRoom.GetComponent<SpriteRenderer>().color = endColor;
                //Removendo a última sala da lista de salas spawnadas, pra poder modificar ela 
                layoutRoomObjects.RemoveAt(layoutRoomObjects.Count - 1);

                endRoom = newRoom;
            }

            selectedDirection = (Direction)Random.Range(0, 4);
            MoveGenerationPoint();

            //Enquanto detectar que tem alguma sala no lugar que a gente quiser spawnar, mudar o ponto de spawn>>>
            while (Physics2D.OverlapCircle(generatorPoint.position, .2f, whatIsRoom))
            {
                MoveGenerationPoint();
            }

        }

        if (includeShop)
        {
            int shopSelector = Random.Range(minDistanceToShop, maxDistanceToShop + 1);
            shopRoom = layoutRoomObjects[shopSelector];
            layoutRoomObjects.RemoveAt(shopSelector);
            shopRoom.GetComponent<SpriteRenderer>().color = shopColor;

        }

        if (includeGunRoom)
        {
            int grSelector = Random.Range(minDistanceToGunRoom, maxDistanceToGunRoom + 1);
            gunRoom = layoutRoomObjects[grSelector];
            layoutRoomObjects.RemoveAt(grSelector);
            gunRoom.GetComponent<SpriteRenderer>().color = grColor;

        }

        //create room outlines
        CreateRoomOutline(Vector3.zero);
        //Para cada sala na lista de gameobjects, criar um outline, que irá definir a posição das portas
        foreach(GameObject room in layoutRoomObjects)
        {
            CreateRoomOutline(room.transform.position);
        }
        CreateRoomOutline(endRoom.transform.position);
        if (includeShop)
        {
            CreateRoomOutline(shopRoom.transform.position);
        }
        if (includeGunRoom)
        {
            CreateRoomOutline(gunRoom.transform.position);
        }


        //Para cada outline, criar um centro da sala, que irá definir os inimigos, obstáculos, etc.
        foreach (GameObject outline in generatedOutlines)
        {
            //Essa bool checa se é necessário gerar um centro da sala selecionada
            bool generateCenter = true;

            if (outline.transform.position == Vector3.zero)
            {
                Instantiate(centerStart, outline.transform.position, transform.rotation).theRoom = outline.GetComponent<Room>();

                generateCenter = false;
            }

            if (outline.transform.position == endRoom.transform.position)
            {
                Instantiate(centerEnd, outline.transform.position, transform.rotation).theRoom = outline.GetComponent<Room>();

                generateCenter = false;
            }

            //Gerando o Shop
            if (includeShop)
            {
                if (outline.transform.position == shopRoom.transform.position)
                {
                    Instantiate(centerShop, outline.transform.position, transform.rotation).theRoom = outline.GetComponent<Room>();

                    generateCenter = false;
                }
            }

            if (includeGunRoom)
            {

                if (outline.transform.position == gunRoom.transform.position)
                {
                    Instantiate(centerGunRoom, outline.transform.position, transform.rotation).theRoom = outline.GetComponent<Room>();

                    generateCenter = false;
                }
            }

            if (generateCenter)
            {
                //Essa int é para escolher um centro aleatório dentro do tamanho do array
                int centerSelect = Random.Range(0, potentialCenters.Length);

                Instantiate(potentialCenters[centerSelect], outline.transform.position, transform.rotation).theRoom = outline.GetComponent<Room>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
#endif
    }

    public void MoveGenerationPoint()
    {
        //switch basicamente é usado para comparar uma variável com muitos fatores, sendo mais ou menos como uma série de "ifs"
        switch (selectedDirection)
        {
            //A syntax dele é assim: em caso de variável xupinga Direction == up, então roda esse código e break; (pra acabar o case)

            case Direction.up:
                generatorPoint.position += new Vector3(0f, yOffSet, 0f);
                break;
            case Direction.down:
                generatorPoint.position += new Vector3(0f, -yOffSet, 0f);
                break;
            case Direction.right:
                generatorPoint.position += new Vector3(xOffSet, 0f, 0f);
                break;
            case Direction.left:
                generatorPoint.position += new Vector3(-xOffSet, 0f, 0f);
                break;
        }
    }
    //Essa função checa se há objetos em cima, embaixo, na direita ou esquerda
    public void CreateRoomOutline(Vector3 roomPosition)
    {
        bool roomAbove = Physics2D.OverlapCircle(roomPosition + new Vector3(0f, yOffSet, 0f), .2f, whatIsRoom);
        bool roomBelow = Physics2D.OverlapCircle(roomPosition + new Vector3(0f, -yOffSet, 0f), .2f, whatIsRoom);
        bool roomLeft = Physics2D.OverlapCircle(roomPosition + new Vector3(-xOffSet, 0f, 0f), .2f, whatIsRoom); 
        bool roomRight = Physics2D.OverlapCircle(roomPosition + new Vector3(xOffSet, 0f, 0f), .2f, whatIsRoom);
        //Para checar se há salas existentes nas direções, deveriam ser feitos uma série de ifs como (se for direita e esquerda x, se for direita e cima y) mas como fazer sem se repetir tanto? Assim:
        //int contador de direções para saber a quantidade de cruzamentos da sala criada
        int directionCount = 0;
        if (roomAbove)
        {
            directionCount++;
        }
        if (roomBelow)
        {
            directionCount++;
        }
        if (roomLeft)
        {
            directionCount++;
        }
        if (roomRight)
        {
            directionCount++;
        }
        //Switch para determinar o que fazer com o contador de direções,comparando o contador com o case(se é 0, 1 , 2, etc) e adicionando para uma lista (generatedOutlines)
        switch (directionCount)
        {
            case 0:
                Debug.LogError("Found no room exits!!");
                break;

            case 1:
                if (roomAbove)
                {
                    generatedOutlines.Add(Instantiate(rooms.singleUp, roomPosition, transform.rotation));
                }
                if (roomBelow)
                {
                    generatedOutlines.Add(Instantiate(rooms.singleDown, roomPosition, transform.rotation));
                }
                if (roomLeft)
                {
                    generatedOutlines.Add(Instantiate(rooms.singleLeft, roomPosition, transform.rotation));
                }
                if (roomRight)
                {
                    generatedOutlines.Add(Instantiate(rooms.singleRight, roomPosition, transform.rotation));
                }

                break;

            case 2:
                if (roomAbove && roomBelow)
                {
                    generatedOutlines.Add(Instantiate(rooms.doubleUpDown, roomPosition, transform.rotation));
                }
                if (roomLeft && roomRight)
                {
                    generatedOutlines.Add(Instantiate(rooms.doubleLeftRight, roomPosition, transform.rotation));
                }
                if(roomAbove && roomRight)
                {
                    generatedOutlines.Add(Instantiate(rooms.doubleUpRight, roomPosition, transform.rotation));
                }
                if (roomRight && roomBelow)
                {
                    generatedOutlines.Add(Instantiate(rooms.doubleRightDown, roomPosition, transform.rotation));
                }
                if (roomBelow && roomLeft)
                {
                    generatedOutlines.Add(Instantiate(rooms.doubleDownLeft, roomPosition, transform.rotation));
                }
                if (roomLeft && roomAbove)
                {
                    generatedOutlines.Add(Instantiate(rooms.doubleLeftUp, roomPosition, transform.rotation));
                }

                break;
                //Checando cada possibilidade de geração nas salas, sentido horário.
            case 3:
                if (roomAbove && roomRight && roomBelow)
                {
                    generatedOutlines.Add(Instantiate(rooms.tripleUpRightDown, roomPosition, transform.rotation));
                }
                if (roomRight && roomBelow && roomLeft)
                {
                    generatedOutlines.Add(Instantiate(rooms.tripleRightDownLeft, roomPosition, transform.rotation));
                }
                if (roomBelow && roomLeft && roomAbove)
                {
                    generatedOutlines.Add(Instantiate(rooms.tripleDownLeftUp, roomPosition, transform.rotation));
                }
                if (roomLeft && roomAbove && roomRight)
                {
                    generatedOutlines.Add(Instantiate(rooms.tripleLeftUpRight, roomPosition, transform.rotation));
                }

                break;

            case 4:
                if (roomBelow && roomLeft && roomAbove && roomRight)
                {
                    generatedOutlines.Add(Instantiate(rooms.fourway, roomPosition, transform.rotation));
                }
                break;
        }

    }
}

[System.Serializable]
public class RoomPrefabs
{
    public GameObject singleUp, singleDown, singleRight, singleLeft,
        doubleUpDown, doubleLeftRight, doubleUpRight, doubleRightDown, doubleDownLeft, doubleLeftUp,
        tripleUpRightDown, tripleRightDownLeft, tripleDownLeftUp, tripleLeftUpRight,
        fourway;
}