using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class is attached to each tilemap segment and stores details like which edges have entrances/exits for the StageManager to check
// the StageManager stores a list of these prefabs
// on initialisation of a stage, a number of the prefabs segments are instantiated

public class TilemapSegment : MonoBehaviour
{
    [field: Header("all tile segments MUST BE 14 x 14")]
    [field: Header("entrance/exits MUST BE at least 2 spaces wide in the centre of an edge")]
    [field: Header("all non-floor spaces MUST BE filled with blocking spaces")]
    [field: SerializeField]public bool exitTop { get; private set; }
    [field: SerializeField]public bool exitRight { get; private set; }
    [field: SerializeField]public bool exitBottom { get; private set; }
    [field: SerializeField]public bool exitLeft { get; private set; }
    [SerializeField]private NavNode navnodePrefab; // used to populate all habitable spaces with nav nodes after instantiation
    private int exitCount = -1; // automatically calculated to work out the number of exits easily
    public bool exitsDone { get; private set; } = false; // tracks if this tilemap has been placed and fully initialised (so it doesn't get checked again)

    // called when a tilemap segment has been placed and only has 1 or less exits (i.e. is a dead end) so it doesn't get checked later
    public void ExitsDone()
    {
        exitsDone = true;
    }

    public int ExitCount()
    {
        if (exitCount < 0)
        {
            exitCount = (exitTop ? 1 : 0) + (exitRight ? 1 : 0) + (exitBottom ? 1 : 0) + (exitLeft ? 1 : 0);
        }
        //Debug.Log("tilemap segment " + gameObject + " has " + exitCount + " exits");
        return exitCount;
    }

    // this is called after a tilemap segment is instantiated to populate all passable tiles of the tilemap segment with NavNode objects
    // this is so these can be used by the AI later for path finding
    public List<NavNode> BuildNavNodes()
    {
        List<NavNode> nodeList = new List<NavNode>();
        for (int x = 0; x < Global.TILEMAPDIMS; x++)
        {
            for (int y = 0; y < Global.TILEMAPDIMS; y++)
            {
                Vector3 pos = transform.position;
                pos.x += ((float)x - ((float)(Global.TILEMAPDIMS - 1) * 0.5f));
                pos.y += ((float)y - ((float)(Global.TILEMAPDIMS - 1) * 0.5f));

                if (Physics2D.OverlapCircle(pos, 0.1f, Global.LayerFloor()))
                {
                    // if there IS a collision with floor (i.e. there is floor here) then this is a viable movement spot
                    // TODO possibly add a double-check to make sure there ISNT wall here too later on if there's any reason to doubt it
                    NavNode newNode = Instantiate(navnodePrefab, pos, Quaternion.identity, transform);
                    nodeList.Add(newNode);
                }
            }
        }
        return nodeList;
    }

    // this picks a direction from this tilemap that another tilemap could be placed in (i.e. (1,0), (-1,0), (0,1) or (0,-1))
    // if a zero vector is returned, this means all options are closed (this shouldn't happen!)
    // this expects there to be a tilemap segment placed in the returned direction and problems happen if you don't do that
    public Vector3 PickAdjacentDirection()
    {
        Vector3 direction = Vector3.zero;

        if (exitsDone) return direction;

        List<Vector3> directionOptions = new List<Vector3>();

        if (exitTop)
        {
            if (!Physics2D.OverlapCircle(transform.position + Vector3.up * Global.TILEMAPDIMS, 1f, Global.LayerTerrain()))
                directionOptions.Add(Vector3.up);
            //else Debug.Log("tilemap " + gameObject + " found obstacle at position " + (transform.position + Vector3.up * Global.TILEMAPDIMS));
        }
        if (exitRight)
        {
            if (!Physics2D.OverlapCircle(transform.position + Vector3.right * Global.TILEMAPDIMS, 1f, Global.LayerTerrain()))
                directionOptions.Add(Vector3.right);
            //else Debug.Log("tilemap " + gameObject + " found obstacle at position " + (transform.position + Vector3.up * Global.TILEMAPDIMS));
        }
        if (exitBottom)
        {
            if (!Physics2D.OverlapCircle(transform.position + Vector3.down * Global.TILEMAPDIMS, 1f, Global.LayerTerrain()))
                directionOptions.Add(Vector3.down);
            //else Debug.Log("tilemap " + gameObject + " found obstacle at position " + (transform.position + Vector3.up * Global.TILEMAPDIMS));
        }
        if (exitLeft)
        {
            if (!Physics2D.OverlapCircle(transform.position + Vector3.left * Global.TILEMAPDIMS, 1f, Global.LayerTerrain()))
                directionOptions.Add(Vector3.left);
            //else Debug.Log("tilemap " + gameObject + " found obstacle at position " + (transform.position + Vector3.up * Global.TILEMAPDIMS));
        }

        if (directionOptions.Count == 0)
        {
            exitsDone = true;
            return direction;
        }

        if (directionOptions.Count == 1)
            exitsDone = true; // assumes that there WILL be a tilemap placed in the new direction, so the last exit is now closed

        return directionOptions[Random.Range(0, directionOptions.Count)];
    }

    // check if this can connect to a tilemap that is connecting IN THE DIRECTION OF the indicated direction vector (so Vector3.right means approaching from the left)
    public bool CanConnect(Vector3 direction)
    {
        if (ExitCount() == 0) return true; // no exits, so this is always an available end cap (shouldn't be needed in the final build!)

        if (direction.x > 0 && exitLeft) return true;
        if (direction.x < 0 && exitRight) return true;
        if (direction.y > 0 && exitBottom) return true;
        if (direction.y < 0 && exitTop) return true;

        return false;
    }
}
