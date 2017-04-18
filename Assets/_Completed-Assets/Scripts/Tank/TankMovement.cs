using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Complete
{
	public class TankMovement : MonoBehaviour
	{
		public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
		public float m_Speed = 12f;                 // How fast the tank moves forward and back.
		public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
		public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
		public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
		public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
		public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

		private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
		private string m_TurnAxisName;              // The name of the input axis for turning.
		private Rigidbody m_Rigidbody;              // Reference used to move the tank.
		private float m_MovementInputValue;         // The current value of the movement input.
		private float m_TurnInputValue;             // The current value of the turn input.
		private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
		private ParticleSystem[] m_particleSystems; // References to all the particles systems used by the Tanks

		private float bensin = 99999;						// ini sisa bensin
		private float bensinAwal = 99999;				// ini bensin awal
		public float maju = 0.05f;					// kecepatan maju
		public float muter;					        // kecepatan muter

        private Vector3 tujuan;

		private void Awake ()
		{
			m_Rigidbody = GetComponent<Rigidbody> ();
		}


		private void OnEnable ()
		{
			// When the tank is turned on, make sure it's not kinematic.
			m_Rigidbody.isKinematic = false;

			// Also reset the input values.
			m_MovementInputValue = 0f;
			m_TurnInputValue = 0f;

			// We grab all the Particle systems child of that Tank to be able to Stop/Play them on Deactivate/Activate
			// It is needed because we move the Tank when spawning it, and if the Particle System is playing while we do that
			// it "think" it move from (0,0,0) to the spawn point, creating a huge trail of smoke
			m_particleSystems = GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < m_particleSystems.Length; ++i)
			{
				m_particleSystems[i].Play();
			}
		}


		private void OnDisable ()
		{
			// When the tank is turned off, set it to kinematic so it stops moving.
			m_Rigidbody.isKinematic = true;

			// Stop all particle system so it "reset" it's position to the actual one instead of thinking we moved when spawning
			for(int i = 0; i < m_particleSystems.Length; ++i)
			{
				m_particleSystems[i].Stop();
			}
		}


		private void Start ()
		{
			// The axes names are based on player number.
			m_MovementAxisName = "Vertical" + m_PlayerNumber;
			m_TurnAxisName = "Horizontal" + m_PlayerNumber;
            UnityWebRequest www = UnityWebRequest.Get("https://tubespbd-48946.firebaseio.com/jovian/username.json");

            StartCoroutine(GetText());
            Debug.Log(PlayerPrefs.GetString("username") + "Bensin: " + bensinAwal);
			bensin = bensinAwal;

			// Store the original pitch of the audio source.
			m_OriginalPitch = m_MovementAudio.pitch;
		}


		private void Update ()
		{
			// Store the value of both input axes.
			if (m_PlayerNumber == 1) { //ini player
				m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
				m_TurnInputValue = Input.GetAxis (m_TurnAxisName);
                PlayerPrefs.SetFloat("cBensin", bensin);
                PlayerPrefs.Save();
			} /*else if (m_PlayerNumber == 2) { //ini nyoba buat AI tapi gagal
				Vector3 rot = transform.rotation.eulerAngles;
				if (rot.y >= 90) { //kondisi
					m_TurnInputValue = -1f;
					//Debug.Log ("Test1");
					//Debug.Log (rot.y);
				} else {
					m_TurnInputValue = 1f;
					//Debug.Log ("Test");
					//Debug.Log (rot.y);
				}
			}*/ else { //ini musuh
				System.Random r = new System.Random();
				m_MovementInputValue = (float) (r.NextDouble () * maju);
				m_TurnInputValue = (float) (r.NextDouble () * 2 * muter - muter);
			}

			EngineAudio ();
		}


		private void EngineAudio ()
		{
			// If there is no input (the tank is stationary)...
			if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
			{
				// ... and if the audio source is currently playing the driving clip...
				if (m_MovementAudio.clip == m_EngineDriving)
				{
					// ... change the clip to idling and play it.
					m_MovementAudio.clip = m_EngineIdling;
					m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
					m_MovementAudio.Play ();
				}
			}
			else
			{
				// Otherwise if the tank is moving and if the idling clip is currently playing...
				if (m_MovementAudio.clip == m_EngineIdling)
				{
					// ... change the clip to driving and play.
					m_MovementAudio.clip = m_EngineDriving;
					m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
					m_MovementAudio.Play();
				}
			}
		}


		private void FixedUpdate ()
		{
			// Adjust the rigidbodies position and orientation in FixedUpdate.
			//if (this.m_PlayerNumber == 1) {

			if (bensin > 0) { //dia bisa gerak cuma kalo bensin masih ada
				Move ();
                if (m_PlayerNumber != 1)
                    transform.LookAt(Vector3.zero);
				Turn ();
			} else {
			    //kalo bensin abis, self destruct
				gameObject.SetActive (false);
			}
			/*} else {
				AutoMove ();
				AutoTurn ();
			}*/
		}


		private void Move ()
		{
            //transform.LookAt(Vector3.zero);
			// Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
			Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

			// Apply this movement to the rigidbody's position.
			m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
			if (m_PlayerNumber==1)
				bensin -= System.Math.Abs(m_MovementInputValue);  //yg ada bates bensinnya cuma player
		}

		//ini nyoba buat AI, tapi gagal
		private void AutoMove ()
		{
			// Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
			Vector3 movement = transform.forward * 2f * m_Speed * Time.deltaTime;

			// Apply this movement to the rigidbody's position.
			m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
		}


		private void Turn ()
		{
			// Determine the number of degrees to be turned based on the input, speed and time between frames.
			float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

			// Make this into a rotation in the y axis.
			Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

			// Apply this rotation to the rigidbody's rotation.
			m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);

			if (m_PlayerNumber==1) //yg ada bates bensinnya cuma player
				bensin -= System.Math.Abs(m_TurnInputValue);
		}

		//ini nyoba buat AI, tapi gagal
		private void AutoTurn ()
		{
			// Determine the number of degrees to be turned based on the input, speed and time between frames.
			float turn = 2f * m_TurnSpeed * Time.deltaTime;

			// Make this into a rotation in the y axis.
			Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

			// Apply this rotation to the rigidbody's rotation.
			m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
		}

        public void setTujuan(Vector3 _tujuan)
        {
            tujuan = _tujuan;
        }

        //FIrebase
        IEnumerator GetText()
        {
            // ini dapetin jsonnya, jadi kalo peluru tinggal ..../user/peluru.json
            // kalo bensin .../user/bensin.json
			UnityWebRequest www = UnityWebRequest.Get("https://fir-auth-af652.firebaseio.com/" + PlayerPrefs.GetString("UID") + ".json");
            //setiap url connection harus pake yiel return ini
            yield return www.Send();

            if (www.isError)
            {
                Debug.Log("ERROR");
            }
            else
            {
                // www.downloadHandler.text -> hasil json nya
                Debug.Log(www.downloadHandler.text);

                // Kalo ini, kalo dapetin json nya 1 user, dia otomatis buat class User yg atributnya nyesuai in sm isi json nya
                UserUnity user = JsonUtility.FromJson<UserUnity>(www.downloadHandler.text);
                bensin = user.bensin;
                Debug.Log("YAY " + bensin);
            }
        }
    }
}