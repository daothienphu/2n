using UnityEngine;
using UnityEngine.UI;

namespace _Project._Scripts.Utilities
{
    public class BgScroller : MonoBehaviour
    {
        [SerializeField] private RawImage _img;
        [SerializeField] private float _x = 0.1f, _y = 0.1f;
        [SerializeField] private float _loadSpeed = 1;
        [SerializeField] private float _w = 15, _h = 15;
        private UIStates _state;


        void Start(){
            _img.uvRect = new Rect(Vector2.zero, Vector2.zero);
            _state = UIStates.LoadIn;
        }

        void Update()
        {
            if (_state == UIStates.Scrolling){
                ScrollingBg();
            }
            else if (_state == UIStates.LoadIn){
                Load();
                if (DoneLoadIn()){
                    _state = UIStates.Scrolling;
                }
            }
            else if (_state == UIStates.LoadOut){
                Load(false);
            }
        }

        void Load(bool loadIn = true){
            var size = _img.uvRect.size;
            size.x = Mathf.Clamp(size.x + (loadIn ? 1 : -1) * _w * Time.deltaTime * _loadSpeed, 0, _w) ;
            size.y = Mathf.Clamp(size.y + (loadIn ? 1 : -1) * _h * Time.deltaTime * _loadSpeed, 0, _h);
            _img.uvRect = new Rect(_img.uvRect.position, size);
        }

        bool DoneLoadIn(){
            var size = _img.uvRect.size;
            return size.x >= _w && size.y >= _h;
        }

        void ScrollingBg(){
            _img.uvRect = new Rect(_img.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _img.uvRect.size);
        }

        public void LoadOutBg(){
            _state = UIStates.LoadOut;
        }
    }

    public enum UIStates{
        LoadIn,
        LoadOut,
        Scrolling
    }
}