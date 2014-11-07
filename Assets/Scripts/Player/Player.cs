using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerRole role;

    public void UpdateRole(string roleString)
    {
        switch (roleString)
        {
            case "Eden":
                role = new Eden();
                break;
            case "Scout":
                role = new Scout();
                break;
            case "Gunner":
                role = new Gunner();
                break;
            case "Monster":
                role = new Monster();
                break;
            default:
                role = new PlayerRole();
                break;
        }
    }
}
