using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TargetCamera : MonoBehaviour {
	// NAMES
	// FOLARANMI OGUNFEMI
	// ERIK BOTSKO
	static public TargetCamera S;
	public bool editMode = true;
	public GameObject fpCamera; // First-person Camera
	// Maximum deviation in Shot.position allowed
	public float maxPosDeviation = 1f;
	// Maximum deviation in Shot.target allowed
	public float maxTarDeviation = 0.5f;
	// Easing for these deviations
	public string deviationEasing = Easing.Out;
	public float passingAccuracy = 0.7f;

	public bool checkToDeletePlayerPrefs = false;

	public bool ________________;

	public Rect camRectNormal; // Pulled from camera.rect

	public int shotNum;
	public GUIText shotCounter, shotRating;
	public GUITexture checkMark;
	public Shot lastShot;
	public int numShots;
	public Shot[] playerShots;
	public float[] playerRatings;
	public GUITexture whiteOut;

	void Awake() {
		S = this;
	}

	void Start() {
		// Find the GUI components
		GameObject go = GameObject.Find("ShotCounter");
		shotCounter = go.GetComponent<GUIText>();
		go = GameObject.Find("ShotRating");
		shotRating = go.GetComponent<GUIText>();
		go = GameObject.Find("_Check_64");
		checkMark = go.GetComponent<GUITexture>();
		go = GameObject.Find ("WhiteOut");
		whiteOut = go.GetComponent<GUITexture>();
		// Hide the checkMark and whiteOut
		checkMark.enabled = false;
		whiteOut.enabled = false;

		// Load all the shots from PlayerPrefs
		Shot.LoadShots();
		// If there were shots stored in PlayerPrefs
		if (Shot.shots.Count>0) {
			shotNum = 0;
			ResetPlayerShotsAndRatings();
			ShowShot(Shot.shots[shotNum]);
		}

		// Hide the cursor (Note: this doesn't work in the Unity Editor unless
		// the Game pane is set to Maximize on Play.)
		Screen.showCursor = false;

		camRectNormal = camera.rect;
	}
/// <summary>
/// Resets the player shots and ratings.
/// </summary>
	/// 
/////////////////[Below] page 660
	void ResetPlayerShotsAndRatings() {
		numShots = Shot.shots.Count;
		// Initialize playerShots & playerRatings with default values
		playerShots = new Shot[numShots];
		playerRatings = new float[numShots];
	}
/////////////////[Above] page 660
	void Update () {
		Shot sh;
		// Mouse Input
////////The below Was replaced on page 655
		/// If Left or Right mouse button is pressed this frame...
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
		// if (Input.GetMouseButtonDown(0)) { // Left mouse button
			sh = new Shot();
			// Grab the position and rotation of fpCamera
			sh.position = fpCamera.transform.position;
			sh.rotation = fpCamera.transform.rotation;
			// Shoot a ray from the camera and see what it hits
			Ray ray = new Ray(sh.position, fpCamera.transform.forward);
			RaycastHit hit;
			if ( Physics.Raycast(ray, out hit) ) {
				sh.target = hit.point;
			}
			if (editMode) {
				if (Input.GetMouseButtonDown(0)) {
					// Left button records a new shot
					Shot.shots.Add(sh);
					shotNum = Shot.shots.Count-1;
					ShowShot (sh);
/////// left click issues
				} else if (Input.GetMouseButtonDown(1)) {
					// Right button replaces the current shot
					Shot.ReplaceShot(shotNum, sh);
					ShowShot(Shot.shots[shotNum]);
				}

////////[Below] not bold in book
				/// bold issue resolved pg 660
				// Reset information about the player when editing shots
				ResetPlayerShotsAndRatings();
////////[Above] not bold in book
			} else {
				// Test this shot against the current Shot
				float acc = Shot.Compare( Shot.shots[shotNum], sh );
				lastShot = sh;
				playerShots[shotNum] = sh;
				playerRatings[shotNum] = acc;
				// Show the shot just taken by the player
				ShowShot(sh);
				// Return to the current shot after waiting 1 second
				Invoke("ShowCurrentShot",1);
			}

			// Play the shutter sound
			this.GetComponent<AudioSource>().Play();

			// Position _TargetCamera with the Shot
/////// The below was commented out on page 655
			// ShowShot(sh);

			Utils.tr( sh.ToXML() );

			// Record a new shot
///	wE DON'T NEED TO RECORD THE SHOT SO COMMENTED OUT
		/// Shot.shots.Add(sh);
///			shotNum = Shot.shots.Count-1;
		}
		// Keyboard Input
		// Use Q and E to cycle Shots
		// Note: Either of these will throw an error if Shot.shots is empty.
		if (Input.GetKeyDown(KeyCode.Q)) {
			shotNum--;
			if (shotNum < 0) shotNum = Shot.shots.Count-1;
			ShowShot(Shot.shots[shotNum]);
		}
		if (Input.GetKeyDown(KeyCode.E)) {
			shotNum++;
			if (shotNum >= Shot.shots.Count) shotNum = 0;
			ShowShot(Shot.shots[shotNum]);
		}
		// If in editMode & Left Shift is held down...
		if (editMode && Input.GetKey(KeyCode.LeftShift)) {
			// Hold Tab to maximize the Target window
			if (Input.GetKeyDown(KeyCode.Tab)) {
				// Maximize when Tab is pressed
				camera.rect = new Rect(0,0,1,1);
			}
			if (Input.GetKeyUp(KeyCode.Tab)) {
				// Return to normal when Tab is released
				camera.rect = camRectNormal;
			}
///// Below may need to beCommented out, see page 656
			// Use Shift-S to Save
			if (Input.GetKeyDown(KeyCode.S)) {
				Shot.SaveShots();
			}
			// Use Shift-X to output XML to Console
			if (Input.GetKeyDown(KeyCode.X)) {
				Utils.tr(Shot.XML);
///// Above may need to be Commented out, see page 656
			}
		}
		// Update the GUITexts
		shotCounter.text = (shotNum+1).ToString()+" of "+Shot.shots.Count;
		if (Shot.shots.Count == 0) shotCounter.text = "No shots exist";
		// ^ Shot.shots.Count doesn't require .ToString() because it is assumed
		// when the left side of the + operator is a string
////////Below commented out pg 661
		// shotRating.text = ""; // This line will be replaced later

		if (playerRatings.Length > shotNum && playerShots[shotNum] != null) {
			float rating = Mathf.Round(playerRatings[shotNum]*100f);
			if (rating < 0) rating = 0;
			shotRating.text = rating.ToString()+"%";
			checkMark.enabled = (playerRatings[shotNum] > passingAccuracy);
			// ^ the > comparison is used to generate true or false
		} else {
			shotRating.text = "";
			checkMark.enabled = false;
		}
		}


	public void ShowShot(Shot sh) {
	// Call WhiteOutTargetWindow() and let it handle its own timing
	StartCoroutine( WhiteOutTargetWindow() );
		// Position _TargetCamera with the Shot
		transform.position = sh.position;
		transform.rotation = sh.rotation;
	}

public void ShowCurrentShot() {
	ShowShot(Shot.shots[shotNum]);
}
// Another use for coroutines is to have a fire-and-forget function with a
// delay in it as we've done here. WhiteOutTargetWindow() will enable
// whiteOut, yield for 0.05 seconds, and then disable it. Compare this
// method of delay to the Invoke("ShowCurrentShot",1f) used above
public IEnumerator WhiteOutTargetWindow() {
	whiteOut.enabled = true;
	yield return new WaitForSeconds(0.05f);
	whiteOut.enabled = false;
}

/////////Page 652
/// 
/// 
// OnDrawGizmos() is called ANY time Gizmos need to be drawn, even when
// Unity isn't playing!
public void OnDrawGizmos() {
	List<Shot> shots = Shot.shots;
	for (int i=0; i<shots.Count; i++) {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(shots[i].position, 0.5f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine( shots[i].position, shots[i].target );
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(shots[i].target, 0.25f);

/////// may need to be moved a bracket lower Page 654 
		// If checkToDeletePlayerPrefs is checked
		if (checkToDeletePlayerPrefs) {
			Shot.DeleteShots(); // Delete all the shots
			// Uncheck checkToDeletePlayerPrefs
			checkToDeletePlayerPrefs = false;
			shotNum = 0; // Set shotNum to 0

/////// may need to be moved a bracket lower pg 659
			// Show the player's last shot attempt
			if (lastShot != null) {
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(lastShot.position, 0.25f);
				Gizmos.color = Color.white;
				Gizmos.DrawLine( lastShot.position, lastShot.target );
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(lastShot.target, 0.125f);
			}// lastshot
		} //checktodeleteplayerprefs
	} // for loop
} //onDrawGizmos
} // public class
