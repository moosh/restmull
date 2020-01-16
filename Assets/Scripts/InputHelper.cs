using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
 
/*****************************************************************************

*****************************************************************************/
public class TouchCreator
 {
     //static BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
     static Dictionary<string, FieldInfo> fields;
 
     object touch;
 
     public float deltaTime { get { return ((Touch)touch).deltaTime; } set { fields["m_TimeDelta"].SetValue(touch, value); } }
     public int tapCount { get { return ((Touch)touch).tapCount; } set { fields["m_TapCount"].SetValue(touch, value); } }
     public TouchPhase phase { get { return ((Touch)touch).phase; } set { fields["m_Phase"].SetValue(touch, value); } }
     public Vector2 deltaPosition { get { return ((Touch)touch).deltaPosition; } set { fields["m_PositionDelta"].SetValue(touch, value); } }
     public int fingerId { get { return ((Touch)touch).fingerId; } set { fields["m_FingerId"].SetValue(touch, value); } }
     public Vector2 position { get { return ((Touch)touch).position; } set { fields["m_Position"].SetValue(touch, value); } }
     public Vector2 rawPosition { get { return ((Touch)touch).rawPosition; } set { fields["m_RawPosition"].SetValue(touch, value); } }
 
     public Touch Create()
     {
         return (Touch)touch;
     }
     
     public TouchCreator()
     {
         touch = new Touch();
     }
 
     static TouchCreator()
     {
         fields = new Dictionary<string, FieldInfo>();
         foreach(var f in typeof(Touch).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
         {
             fields.Add(f.Name, f);
            // Debug.Log("name: " + f.Name);
         }
     }
 }
 
/*****************************************************************************

*****************************************************************************/
public class InputHelper : MonoBehaviour
 {
 	public delegate bool SwipeEventHandler(Vector2 inSwipeVector); // Returns TRUE if event was consumed and should NOT be propagated further
 	public SwipeEventHandler swipeHandler;
 	
    private bool mDetectSwipeOnRelease = true;
    private Vector2 mFingerDown;
    private Vector2 mFingerUp;
    private static TouchCreator lastFakeTouch;
 
	/*****************************************************************************

	*****************************************************************************/
     public static List<Touch> GetTouches()
     {
         List<Touch> touches = new List<Touch>();
         touches.AddRange(Input.touches);

         if(lastFakeTouch == null) lastFakeTouch = new TouchCreator();
         if(Input.GetMouseButtonDown(0))
         {
             lastFakeTouch.phase = TouchPhase.Began;
             lastFakeTouch.deltaPosition = new Vector2(0,0);
             lastFakeTouch.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
             lastFakeTouch.fingerId = 0;
         }
         else if (Input.GetMouseButtonUp(0))
         {
             lastFakeTouch.phase = TouchPhase.Ended;
             Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
             lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
             lastFakeTouch.position = newPosition;
             lastFakeTouch.fingerId = 0;
         }
		else if (Input.GetMouseButton(0))
		{
			//lastFakeTouch.phase = TouchPhase.Moved;
			Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
			lastFakeTouch.phase = lastFakeTouch.deltaPosition.magnitude == 0 ? TouchPhase.Stationary : TouchPhase.Moved;
			lastFakeTouch.position = newPosition;
			lastFakeTouch.fingerId = 0;
		}
        else
         {
             lastFakeTouch = null;
         }
         if (lastFakeTouch != null) touches.Add(lastFakeTouch.Create());
            
  		return touches;      
     }
 
 
	/*****************************************************************************

	*****************************************************************************/
 	public void update()
 	{
		if (EventSystem.current.currentSelectedGameObject != null)
			goto done;
			
		foreach (Touch touch in GetTouches())
		{        	
           if (touch.phase == TouchPhase.Began)
			{
				mFingerUp = touch.position;
				mFingerDown = touch.position;
			}

            //Detects Swipe while finger is still moving
            if (touch.phase == TouchPhase.Moved)
            {
                if (!mDetectSwipeOnRelease)
                {
                    mFingerDown = touch.position;
                    checkSwipe();
                }
            }

            //Detects swipe after finger is released
            if (touch.phase == TouchPhase.Ended)
            {
                mFingerDown = touch.position;
                checkSwipe();
            }
        }
        
    done:
    ;
 	}
 	
 	/*****************************************************************************

	*****************************************************************************/
    void checkSwipe()
	{
		if (swipeHandler != null)
		{
			Vector2 v;
			v.x = touchDeltaX();
			v.y = touchDeltaY();
			swipeHandler(v);
		}
    }

	/*****************************************************************************

	*****************************************************************************/
    float touchDeltaY()
    {
        return (mFingerDown.y - mFingerUp.y);
    }

	/*****************************************************************************

	*****************************************************************************/
    float touchDeltaX()
    {
        return (mFingerDown.x - mFingerUp.x);
    } 	
 }
 
 
 
