using System.Collections;

using System.Collections.Generic;
using UnityEngine;

public class ObjectTransformHandler: MonoBehaviour
{
    [SerializeField] private GameObject _ARObject;
    [SerializeField] private Camera ARcamera;

    [SerializeField] private float _speedMovement = 1.0f;
    [SerializeField] private float _speedRotation = 2.0f;
    [SerializeField] private float _scaleFactor = 0.1f;

    private Vector2 _initialTouchPos;
    private Vector2 _touchPositionDiff;

    private float _screenFactor = 0.001f;
    private float _touchDistance;
    private float _rotationTolerance = 1.5f;
    private float _scaleTolerance = 25f;

    private bool _isARObjectSelected;

    private string _tagARObjects = "ARObject";

    void Update()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }
        Touch touchOne = Input.GetTouch(0);
        if (Input.touchCount == 1)
        {
            MoveARObject(touchOne);
        }
        if (Input.touchCount == 2)
        {
            Touch touchTwo = Input.GetTouch(1);
            (float DiffDistanceOrAngle, bool MustScale) = SelectScaleORotate(touchOne, touchTwo);
            if(!MustScale) 
            {
                RotateARObject(DiffDistanceOrAngle);
            }
            else
            {
                ScaleARObject(DiffDistanceOrAngle);
            }
        }
    }

    private void MoveARObject(Touch touchOne)
    {

        if (touchOne.phase == TouchPhase.Began)
        {
            _initialTouchPos = touchOne.position;
            _isARObjectSelected = CheckTouchInARObject(_initialTouchPos);
        }
        if (touchOne.phase == TouchPhase.Moved && _isARObjectSelected)
        {
            Vector2 diffpos = (touchOne.position - _initialTouchPos) * _screenFactor;
            _ARObject.transform.position = _ARObject.transform.position + new Vector3(diffpos.x * _speedMovement, diffpos.y * _speedMovement, 0);
            _initialTouchPos = touchOne.position;
        }
    }
    private (float,bool) SelectScaleORotate(Touch touchOne,Touch touchTwo)
    {
        bool MustScale = false;
        if (touchOne.phase == TouchPhase.Began || touchTwo.phase == TouchPhase.Began)
        {
            _touchPositionDiff = touchOne.position - touchTwo.position;
            _touchDistance = Vector2.Distance(touchTwo.position, touchOne.position);

        }
        if (touchOne.phase == TouchPhase.Moved || touchTwo.phase == TouchPhase.Moved)
        {
            Vector2 currentTouchPositionDiff = touchTwo.position - touchOne.position;
            float currentTouchDistance = Vector2.Distance(touchTwo.position, touchOne.position);
            float diffDistance = currentTouchDistance - _touchDistance;
            float angle = Vector2.SignedAngle(_touchPositionDiff, currentTouchPositionDiff);

            if (Mathf.Abs(diffDistance) > _scaleTolerance)
            {
                MustScale = true;
                return (diffDistance, MustScale);
            }
            
            if (Mathf.Abs(angle) > _rotationTolerance)
            {
                MustScale = false;
                return (angle, MustScale);
            }
            _touchDistance = currentTouchDistance;
            _touchPositionDiff = currentTouchPositionDiff;
            return (0,false);
        }
        else
        {
            return (0,false);
        }
    }
    private void ScaleARObject(float diffDistance)
    {
        Vector3 newscale = _ARObject.transform.localScale + Mathf.Sign(diffDistance) * Vector3.one * _scaleFactor;
        _ARObject.transform.localScale = Vector3.Lerp(_ARObject.transform.localScale, newscale, 0.05f);
    }
    private void RotateARObject(float angle)
    {
        _ARObject.transform.rotation = Quaternion.Euler(0, _ARObject.transform.rotation.eulerAngles.y - Mathf.Sign(angle) * _speedRotation, 0);
    }
    private bool CheckTouchInARObject(Vector2 touchPosition)
    {
        Ray ray = ARcamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hitARObject))
        {
            if (hitARObject.collider.CompareTag(_tagARObjects))
            {
                _ARObject = hitARObject.transform.gameObject;
                return true;
            }
        }
        return false;
    }

}
