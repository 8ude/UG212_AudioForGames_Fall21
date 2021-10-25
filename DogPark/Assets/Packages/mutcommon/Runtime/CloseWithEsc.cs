using UnityEngine;

namespace MutCommon
{
  public class CloseWithEsc : MonoBehaviour
  {
    // Start is called before the first frame update
    void Awake()
    {
      System.Console.WriteLine("AAAA");
      DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
        CloseGame();
      }
    }

    public void CloseGame()
    {
      Debug.Log("Closing game");
      Application.Quit();
    }
  }
}