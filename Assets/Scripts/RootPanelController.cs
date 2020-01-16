using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RootPanelController : MonoBehaviour
{
	public InputHelper inputHelper; // Detects and reports click/drag operations
	public Button playButton;
	public Image blueBin, greenBin, yellowBin, gamePiece;
	public Text scoreText;
	
	private Sprite mGlassBottleSprite;
	private Sprite mPlasticBottleSprite;
	private Sprite mPaperSprite;
	private Animator mAnimator;
	private Vector2 mGamePieceStartingCenterPos, mGamePieceEndingCenterPos;	// Resolution-independent center position, relative to parent container
	private Vector2 mGamePieceRadiusRI; // Res-independent game piece radius (in the range of [0,1], relative to parent game object container)
	private float mAnimationIndex = 0.0f; // Used for animating the game piece. ranges in [0,1]
	private bool mDoAnim = false;
	private int mScore = 0;
	
	/*****************************************************************************

	*****************************************************************************/
	void Awake()
	{
		inputHelper.swipeHandler = onSwipe;
	}
	
	/*****************************************************************************

	*****************************************************************************/
    void Start()
    {
    	mAnimator 				= gameObject.GetComponent<Animator>();
		mGlassBottleSprite		= Resources.Load<Sprite>("glass-bottle");
		mPlasticBottleSprite	= Resources.Load<Sprite>("plastic-bottle");
		mPaperSprite			= Resources.Load<Sprite>("paper");

		playButton.onClick.AddListener(delegate
		{
			mAnimator.SetTrigger("doPlayButtonBounce");
			showNextPiece();
		});	

		// Game piece size does not change. We only need to capture this info once
		RectTransform rtGame = gamePiece.GetComponent<RectTransform>();
		mGamePieceRadiusRI.x = (rtGame.anchorMax.x - rtGame.anchorMin.x) / 2.0f;
		mGamePieceRadiusRI.y = (rtGame.anchorMax.y - rtGame.anchorMin.y) / 2.0f;
		
		mGamePieceStartingCenterPos.x = (rtGame.anchorMax.x + rtGame.anchorMin.x) / 2.0f;
		mGamePieceStartingCenterPos.y = (rtGame.anchorMax.y + rtGame.anchorMin.y) / 2.0f;
		
		mAnimator.SetTrigger("doPlayButtonWiggle");	

    }

	/*****************************************************************************

	*****************************************************************************/
    void Update()
    {
		inputHelper.update();
		
		float time = 0.15f;
		if (mDoAnim)
		{
			RectTransform rt = gamePiece.GetComponent<RectTransform>();
			Vector2 vMin, vMax;
			float rate = 1.0f / time;
			
			if (mAnimationIndex < 1.0f)
			{
				mAnimationIndex += Time.deltaTime * rate;

				Vector2 currentCenter = Vector2.Lerp(mGamePieceStartingCenterPos, mGamePieceEndingCenterPos, mAnimationIndex);
				
				// Since we keep track of the game piece's CENTER, we need to rebuild
				// the anchorMin and anchorMax from that center value. Do this by adding
				// or subtracting half the game piece size (aka its radius).
				vMin.x = currentCenter.x - mGamePieceRadiusRI.x;
				vMin.y = currentCenter.y - mGamePieceRadiusRI.y;
				vMax.x = currentCenter.x + mGamePieceRadiusRI.x;
				vMax.y = currentCenter.y + mGamePieceRadiusRI.y;
				rt.anchorMin = vMin;
				rt.anchorMax = vMax;
			}
			else
			{
				// Done animating, so hide the game piece and return it to the center
				// of the game (its starting position)
				mDoAnim = false;
				mAnimationIndex = 0;
				
				mAnimator.SetTrigger("hideGamePiece");
				vMin.x = mGamePieceStartingCenterPos.x - mGamePieceRadiusRI.x;
				vMin.y = mGamePieceStartingCenterPos.y - mGamePieceRadiusRI.y;
				vMax.x = mGamePieceStartingCenterPos.x + mGamePieceRadiusRI.x;
				vMax.y = mGamePieceStartingCenterPos.y + mGamePieceRadiusRI.y;
				rt.anchorMin = vMin;
				rt.anchorMax = vMax;
				showNextPiece();
			}
		}
    }

	/*****************************************************************************

	*****************************************************************************/
    bool onSwipe(Vector2 inSwipeVector)
    {    	 		
 		moveGamePiece(inSwipeVector);
 
    	return true;
    }

	/*****************************************************************************

	*****************************************************************************/
	void showNextPiece()
	{
		Sprite s = null;
		int rand = UnityEngine.Random.Range(0,3); // random number between [0,2]
		switch (rand)
		{
			case 0:
				s = mGlassBottleSprite;
				break;
			case 1:
				s = mPlasticBottleSprite;
				break;
			case 2:
				s = mPaperSprite;
				break;
			default:
				s = null;
				break;
		}
		
		gamePiece.GetComponent<Image>().sprite = s;
		mAnimator.SetTrigger("showGamePiece");	
	}
	
	/*****************************************************************************

	*****************************************************************************/
	void moveGamePiece(Vector2 inDirection)
	{
		RectTransform rtBin;
		
    	if (inDirection.x < 0 && inDirection.y > 0)
    	{
    		// Move toward blue bin
    		Debug.Log("Moving toward blue bin");
    		rtBin = blueBin.GetComponent<RectTransform>();
			mGamePieceEndingCenterPos.x = (rtBin.anchorMax.x + rtBin.anchorMin.x) / 2.0f;
			mGamePieceEndingCenterPos.y = (rtBin.anchorMax.y + rtBin.anchorMin.y) / 2.0f;
			mDoAnim = true;
			
			if (gamePiece.GetComponent<Image>().sprite == mPlasticBottleSprite)
				mScore += 1;
    	}
    	
    	else if (inDirection.x > 0 && inDirection.y > 0)
    	{
    		// Move toward green bin
     		Debug.Log("Moving toward green bin");
	   		rtBin = greenBin.GetComponent<RectTransform>();
			mGamePieceEndingCenterPos.x = (rtBin.anchorMax.x + rtBin.anchorMin.x) / 2.0f;
			mGamePieceEndingCenterPos.y = (rtBin.anchorMax.y + rtBin.anchorMin.y) / 2.0f;
			mDoAnim = true;

			if (gamePiece.GetComponent<Image>().sprite == mPaperSprite)
				mScore += 1;
    	}
    	
    	else if (inDirection.x > 0 && inDirection.y < 0)
    	{
    		// Move toward yellow bin
     		Debug.Log("Moving toward yellow bin");
   			rtBin = yellowBin.GetComponent<RectTransform>();
			mGamePieceEndingCenterPos.x = (rtBin.anchorMax.x + rtBin.anchorMin.x) / 2.0f;
			mGamePieceEndingCenterPos.y = (rtBin.anchorMax.y + rtBin.anchorMin.y) / 2.0f;
			mDoAnim = true;

			if (gamePiece.GetComponent<Image>().sprite == mGlassBottleSprite)
				mScore += 1;
    	}
    	
    	scoreText.text = String.Format("{0}", mScore);
	}
}

/*****************************************************************************

*****************************************************************************/
/*****************************************************************************

*****************************************************************************/
/*****************************************************************************

*****************************************************************************/
/*****************************************************************************

*****************************************************************************/
