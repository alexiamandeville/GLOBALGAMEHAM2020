using UnityEngine;

namespace Doors
{
  public class LinkedDoors : MonoBehaviour
  {
    public BoxCollider Door1;
    public BoxCollider Door2;

    public BoxCollider[] GetDoors()
    {
      return new[] {Door1, Door2,};
    }
  }
}