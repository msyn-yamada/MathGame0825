using UnityEngine.SceneManagement;
using UnityEngine;


namespace Main
{
    internal sealed class SceneReload : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}

namespace Main.data {
    internal sealed class SceneReload
    {
        // 撃破した車の数の初期値
        public int breakEnemy = 0;
        public void Start()
        {
            
        }
    }
}
