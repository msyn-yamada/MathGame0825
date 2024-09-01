using General.Extension;
using Main.Data;
using Main.Data.Formula;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Main.Handler
{
    internal sealed class UnNumberSpriteFollow : MonoBehaviour
    {
        [SerializeField] private SymbolType _type;
        internal SymbolType Type => _type;

        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField] private SpriteFollow prefab;
        private UnNumberSpriteFollow thisInstance = null;

        [SerializeField, Header("コピーインスタンスのz座標")] private float thisZ;
        [SerializeField, Header("インスタンスのz座標")] private float z;

        private bool isFollowingMouse = false;

        private void OnEnable()
        {
            eventTrigger.AddListener(EventTriggerType.PointerDown, OnPointerDown);
            eventTrigger.AddListener(EventTriggerType.PointerUp, OnPointerUp);
        }

        private void OnDisable()
        {
            eventTrigger = null;
            prefab = null;
            thisInstance = null;
        }

        private void Update()
        {
            if (isFollowingMouse)
            {
                if (thisInstance == null) return;

                thisInstance.transform.position = MouseToWorld(z);
            }
        }

        private void OnPointerDown()
        {
            if (GameManager.Instance.State != GameState.OnGoing) return;

            if (thisInstance != null) return;

            isFollowingMouse = true;
            thisInstance = Instantiate(this, MouseToWorld(z), Quaternion.identity, transform);
            thisInstance.transform.localScale = Vector3.one;
        }

        private void OnPointerUp()
        {
            if (GameManager.Instance.State != GameState.OnGoing) return;

            if (thisInstance == null) return;

            isFollowingMouse = false;

            thisInstance.transform.position.ToVector2().JudgeAttachable
                ((p =>
                {
                    Vector3 toPos = p.ToVector3(z);
                    int toIndex = GameManager.Instance.GetIndexFromSymbolPosition(toPos);

                    if (GameManager.Instance.Formula.Data[toIndex] != Symbol.NONE) return;

                    SpriteFollow instance = Instantiate(prefab, toPos, Quaternion.identity, transform.parent);
                    // 演算子orかっこの前提
                    instance.transform.localScale =
                    Symbol.IsOperator(Type.GetSymbol()) == true ? new(0.5f, 0.5f, 1) : Vector3.one;

                    GameManager.Instance.Formula.Data[toIndex] = Type.GetSymbol();
                    GameManager.Instance.FormulaInstances[toIndex] = instance;
                }),
                (p =>
                {
                    Extension.Pass();
                }));

            Destroy(thisInstance.gameObject);
            thisInstance = null;
        }

        private Vector3 MouseToWorld(float z)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = z;
            return pos;
        }
    }
}