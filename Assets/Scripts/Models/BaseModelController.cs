using UnityEngine;

namespace Models
{
  public class BaseModelController
  {
    public Material firstMaterial;
    public Material secondMaterial;

    public MeshRenderer renderer;

    protected PlayerNum curNum;

    public enum PlayerNum
    {
      INVALID = 0,
      
      FIRST,
      SECOND,
    }

    public void SetPlayerNum(PlayerNum num)
    {
      curNum = num;
    }

    void Start()
    {
      switch (curNum)
      {
        case PlayerNum.FIRST:
          renderer.material = firstMaterial;
          break;
        
        case PlayerNum.SECOND:
          renderer.material = secondMaterial;
          break;
        
        default:
          renderer.material = firstMaterial;
          break;
      }
    }
  }
}