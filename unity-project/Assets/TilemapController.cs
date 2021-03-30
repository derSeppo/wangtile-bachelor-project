using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class TilemapController : MonoBehaviour
{
    public Tilemap tilemap;
    public int mapSize, minAreaSize, maxAreaSize;
    
    private Dictionary<string, Sprite> wangSprites;

    // Start is called before the first frame update
    void Start()
    {

        wangSprites = new Dictionary<string, Sprite>();
        Object[] sprites = Resources.LoadAll("Sprites", typeof(Sprite));

        foreach(var i in sprites) { 
            wangSprites.Add(i.name, (Sprite) i);
        }

        FillTiles();
    }

    private void FillTiles()
    {
        Vector3Int centerCell = tilemap.WorldToCell(Camera.main.transform.position);

        for (int i = -mapSize; i <= mapSize; i++)
        {
            for (int j = -mapSize; j <= mapSize; j++)
            {
                Vector3Int currentLocation = centerCell + new Vector3Int(i, j, 0);

                if (!(tilemap.GetTile(currentLocation) is WangTile))
                {
                    if (SelectSprite(currentLocation, out Sprite sprite))
                    {
                        SetTile(currentLocation, sprite);
                        if (sprite.name[0] == sprite.name[1] && sprite.name[0] == sprite.name[2] && sprite.name[0] == sprite.name[3])
                        {
                            Vector2Int mapLimitDistance = new Vector2Int(mapSize - i, mapSize - j);
                            FillArea(currentLocation, sprite.name[0], mapLimitDistance);
                        }
                    }
                }
            }
        }
    }

    private bool SelectSprite(Vector3Int currentLocation, out Sprite sprite)
    {
        string boundNames = GetSurroundingBoundaries(currentLocation);

        Dictionary<Sprite,List<int>> possibleSpritesAndRotations = new Dictionary<Sprite, List<int>>();

        foreach(string i in wangSprites.Keys)
        {
            int rotation = 0;

            while(rotation<4)
            {
                if ((boundNames[0].Equals('X') || boundNames[0].Equals(i[0])) && (boundNames[1].Equals('X') || boundNames[1].Equals(i[1])) && (boundNames[2].Equals('X') || boundNames[2].Equals(i[2])) && (boundNames[3].Equals('X') || boundNames[3].Equals(i[3])))
                {
                    if (!possibleSpritesAndRotations.ContainsKey(wangSprites[i])){
                        possibleSpritesAndRotations.Add(wangSprites[i], new List<int>());
                    }

                    possibleSpritesAndRotations[wangSprites[i]].Add(rotation);
                }
                rotation++;
                boundNames = boundNames.ShiftRight(1);
            }
        }

        if (possibleSpritesAndRotations.Count > 0)
        {
            Sprite[] possibleSprites = new Sprite[possibleSpritesAndRotations.Keys.Count];
            possibleSpritesAndRotations.Keys.CopyTo(possibleSprites, 0);

            Sprite selectedSprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
            int selectedRotation = possibleSpritesAndRotations[selectedSprite][Random.Range(0, possibleSpritesAndRotations[selectedSprite].Count)];

            sprite = RotateSprite(selectedSprite, selectedRotation);
            return true;
        }
        else
        {
            sprite = null;
            return false;
        }
    }

    private void FillArea(Vector3Int currentLocation, char type, Vector2Int mapLimitDistance)
    {
        Vector2Int areaSize = new Vector2Int(Mathf.Clamp(Random.Range(minAreaSize, maxAreaSize), 0, mapLimitDistance.x), Mathf.Clamp(Random.Range(minAreaSize, maxAreaSize), 0, mapLimitDistance.y));
        string boundNames;
        Vector3Int nextLocation;

        for (int i = 0; i <= areaSize.x; i++)
        {
            for (int j = 0; j <= areaSize.y; j++)
            {
                if(!(i == 0 && j == 0))
                {
                    nextLocation = currentLocation + new Vector3Int(i, j, 0);
                    boundNames = GetSurroundingBoundaries(nextLocation);

                    if (!(tilemap.GetTile(nextLocation) is WangTile))
                    {
                        if ((boundNames[0].Equals('X') || boundNames[0].Equals(type)) && (boundNames[1].Equals('X') || boundNames[1].Equals(type)) && (boundNames[2].Equals('X') || boundNames[2].Equals(type)) && (boundNames[3].Equals('X') || boundNames[3].Equals(type)))
                        {
                            SetTile(nextLocation, wangSprites[new string(type, 4)]);
                        }
                    }
                }
            }
        }
    }

    private void SetTile(Vector3Int currentLocation, Sprite sprite)
    {
        WangTile tile = ScriptableObject.CreateInstance<WangTile>();
        tile.sprite = sprite;
        tile.SetBounds(tile.sprite.name);
        tilemap.SetTile(currentLocation, tile);
    }

    private Sprite RotateSprite(Sprite sprite, int rotation)
    {
        if (rotation <= 0)
        {
            return sprite;
        }
        else
        {
            int width = sprite.texture.width;
            int height = sprite.texture.height;


            Texture2D resultTexture = new Texture2D(64, 64)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            for (int i = 0; i <= width; i++)
            {
                for (int j = 0; j <= height; j++)
                {
                    resultTexture.SetPixel(width - j, i, sprite.texture.GetPixel(i, j));
                }
            }
            resultTexture.Apply();

            Sprite result = Sprite.Create(resultTexture, new Rect(0.0f, 0.0f, resultTexture.width, resultTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            result.name = sprite.name.ShiftLeft(1);

            rotation--;

            return RotateSprite(result, rotation);
        }
    }

    private String GetSurroundingBoundaries(Vector3Int tileLocation)
    {
        string boundNames = "";

        Vector3Int northTile = new Vector3Int(0, 1, 0);
        Vector3Int eastTile = new Vector3Int(1, 0, 0);
        Vector3Int southTile = new Vector3Int(0, -1, 0);
        Vector3Int westTile = new Vector3Int(-1, 0, 0);

        if (tilemap.GetTile(tileLocation + northTile) is WangTile)
        {
            WangTile tile = (WangTile)tilemap.GetTile(tileLocation + northTile);
            boundNames += tile.South;
        }
        else
        {
            boundNames += "X";
        }
        if (tilemap.GetTile(tileLocation + eastTile) is WangTile)
        {
            WangTile tile = (WangTile)tilemap.GetTile(tileLocation + eastTile);
            boundNames += tile.West;
        }
        else
        {
            boundNames += "X";
        }
        if (tilemap.GetTile(tileLocation + southTile) is WangTile)
        {
            WangTile tile = (WangTile)tilemap.GetTile(tileLocation + southTile);
            boundNames += tile.North;
        }
        else
        {
            boundNames += "X";
        }
        if (tilemap.GetTile(tileLocation + westTile) is WangTile)
        {
            WangTile tile = (WangTile)tilemap.GetTile(tileLocation + westTile);
            boundNames += tile.East;
        }
        else
        {
            boundNames += "X";
        }

        return boundNames;
    }

    public void ResetTilemap()
    {
        tilemap.ClearAllTiles();
        FillTiles();
    }

    public void SetMapSize(String input)
    {
        int newMapSize;
        if (int.TryParse(input, out newMapSize))
        {
            if (newMapSize >= 0)
            {
                mapSize = newMapSize;
            }
        }
    }

    public void SetMinAreaSize(String input)
    {
        int newMinAreaSize;
        if (int.TryParse(input, out newMinAreaSize))
        {
            if (newMinAreaSize >= 0)
            {
                minAreaSize = newMinAreaSize;
            }
        }
    }

    public void SetMaxAreaSize(String input)
    {
        int newMaxAreaSize;
        if (int.TryParse(input, out newMaxAreaSize))
        {
            if (newMaxAreaSize >= 0)
            {
                maxAreaSize = newMaxAreaSize;
            }
        }
    }
}

public class WangTile : Tile
{
    private char north, east, south, west;

    public char North { get => north;}
    public char East { get => east;}
    public char South { get => south;}
    public char West { get => west;}

    public void SetBounds(string NESW)
    {
        north = NESW[0];
        east = NESW[1];
        south = NESW[2];
        west = NESW[3];
    }
}

public static class StringExtension
{
    public static string ShiftLeft(this string s, int count)
    {
        return s.Remove(0, count) + s.Substring(0, count);
    }

    public static string ShiftRight(this string s, int count)
    {
        return s.Substring(s.Length - count, count) + s.Remove(s.Length - count, count);
    }
}