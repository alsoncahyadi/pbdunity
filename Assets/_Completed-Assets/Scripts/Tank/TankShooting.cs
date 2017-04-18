using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.IO.Ports;

namespace Complete
{
	public class TankShooting : MonoBehaviour
	{
		public int m_PlayerNumber = 1;              // Used to identify the different players.
		public Rigidbody m_Shell;                   // Prefab of the shell.
		public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
		public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
		public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
		public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
		public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
		public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
		public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
		public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
		public float delay = 2; //detik delay tank menembak


		private string m_FireButton;                // The input axis that is used for launching shells.
		private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
		private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
		private bool m_Fired;                       // Whether or not the shell has been launched with this button press.

		private int peluru = 99999;							// jumlah peluru sisa
		private int peluruAwal = 99999;					// peluru awal

		private float waktu;						// ini waktu yg diincrement
		public float rangeWaktu = 2f;				// ini range waktu nembak, musuh

        private SerialPort sp;                       //port

        private void OnEnable()
		{
			// When the tank is turned on, reset the launch force and the UI
			m_CurrentLaunchForce = m_MinLaunchForce;
			m_AimSlider.value = m_MinLaunchForce;
		}


		private void Start ()
		{
            StartCoroutine(GetText());
            Debug.Log("Peluru Awal: " + peluruAwal);
			peluru = peluruAwal;
            PlayerPrefs.SetInt("cPeluru", peluru);
            PlayerPrefs.Save();
            waktu = Random.Range(0.0f, 1.5f);
			// The fire axis is based on the player number.
			m_FireButton = "Fire" + m_PlayerNumber;
            m_Fired = true;

			// The rate that the launch force charges up is the range of possible forces by the max charge time.
			m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

            //init serial port
            sp = new SerialPort("COM3", 9600);
            sp.Open();
            sp.ReadTimeout = 100;
        }


		private void Update ()
		{
            bool btnPressed = false;
            char btnPressedC = ' ';
            int ultraValue = 0;
            string received = "";
            if (sp.IsOpen)
            {
                //Debug.Log("=== SERIAL OPENED ===");
                try
                {
                    received = sp.ReadLine();
                    btnPressedC = received[0];
                    ultraValue = int.Parse(received.Substring(2));
                    if (ultraValue != 0)
                    {
                        m_CurrentLaunchForce = ultraValue * 2;
                    }
                    m_AimSlider.value = m_CurrentLaunchForce;
                    
                    if (btnPressedC.Equals('1') || btnPressedC.ToString().Equals("1"))
                    {
                        btnPressed = true;
                    }
                    else
                    {
                        btnPressed = false;
                    }
                }
                catch (System.Exception)
                {

                }
                Debug.Log("BTN: '" + btnPressedC + "' | " + "bool: " + btnPressed + " | Ultra: " + ultraValue.ToString());
                //Debug.Log(received);
                //Debug.Log("Ultra: " + ultraValue.ToString());
            }
            
            
			if (m_PlayerNumber == 1) { //ini yg dikontrol player
				// The slider should have a default value of the minimum launch force.
				m_AimSlider.value = m_MinLaunchForce;

				// If the max force has been exceeded and the shell hasn't yet been launched...
				if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired) {
					// ... use the max force and launch the shell.
					m_CurrentLaunchForce = m_MaxLaunchForce;
					Fire ();
				}
				// Otherwise, if the fire button has just started being pressed...
				else if (/*Input.GetButtonDown (m_FireButton) ||*/ btnPressed) {
					// ... reset the fired flag and reset the launch force.
					m_Fired = false;
					m_CurrentLaunchForce = m_MinLaunchForce;

					// Change the clip to the charging clip and start it playing.
					m_ShootingAudio.clip = m_ChargingClip;
					m_ShootingAudio.Play ();
				}
				// Otherwise, if the fire button is being held and the shell hasn't been launched yet...
				else if ((/*Input.GetButton (m_FireButton) /*||*/ btnPressed) && !m_Fired) {
					// Increment the launch force and update the slider.
					m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

					m_AimSlider.value = m_CurrentLaunchForce;
				}
				// Otherwise, if the fire button is released and the shell hasn't been launched yet...
				else if ((/*Input.GetButtonUp (m_FireButton) /*||*/ !btnPressed) && !m_Fired) {
					// ... launch the shell.
					Fire ();
				}
                PlayerPrefs.SetInt("cPeluru", peluru);
                PlayerPrefs.Save();
                sp.Write(peluru.ToString());
            } else { // ini musuh
				if (waktu > rangeWaktu) {
					Fire ();
					waktu = 0f;
				} else
					waktu += Time.deltaTime;
				//StartCoroutine(FireWait ());


			}
		}


		private void Fire ()
		//do nothing kalo peluru abis, kalo masih ada, nembak
		{
			if ((m_PlayerNumber==1)&&(peluru <= 0)) {
				//do nothing
			} else {
				// Set the fired flag so only Fire is only called once.
				m_Fired = true;

				// Create an instance of the shell and store a reference to it's rigidbody.
				Rigidbody shellInstance =
					Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

				// Set the shell's velocity to the launch force in the fire position's forward direction.
				shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward; 

				// Change the clip to the firing clip and play it.
				m_ShootingAudio.clip = m_FireClip;
				m_ShootingAudio.Play ();

				// Reset the launch force.  This is a precaution in case of missing button events.
				m_CurrentLaunchForce = m_MinLaunchForce;

				peluru--;
               
			}
		}

		private IEnumerator FireWait()
		{
			Fire ();
			yield return StartCoroutine (waitsec());
		}

		private IEnumerator waitsec() {
			yield return new WaitForSeconds(5);
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
                peluru = user.peluru;
                Debug.Log("YAY " + peluru);
            }
        }
    }
}