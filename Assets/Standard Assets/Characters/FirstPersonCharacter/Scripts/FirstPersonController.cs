using System;
using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
	{


		public static int score = 0;


		[SerializeField] private bool m_IsWalking;
		[SerializeField] private bool m_IsSquat;
		[SerializeField] private float m_WalkSpeed;
		[SerializeField] private float m_RunSpeed;
		[SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
		[SerializeField] private float m_JumpSpeed;
		[SerializeField] private float m_StickToGroundForce;
		[SerializeField] private float m_GravityMultiplier;
		[SerializeField] private MouseLook m_MouseLook;
		[SerializeField] private bool m_UseFovKick;
		[SerializeField] private FOVKick m_FovKick = new FOVKick();
		[SerializeField] private bool m_UseHeadBob;
		[SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
		[SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
		[SerializeField] private float m_StepInterval;
		[SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
		[SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
		[SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.
		[SerializeField] private AudioClip m_gunSound;    
		[SerializeField] private AudioClip m_reloadSound;  
		[SerializeField] private float m_squatspeed = 1;
		[SerializeField] private GameObject m_particlePrefab;
		[SerializeField] private GameObject m_Ak;
		[SerializeField] private GameObject m_muzzle;
		[SerializeField] private int m_bulletLimit = 30;
		[SerializeField] private int m_bulletBox = 250;
		[SerializeField] private GUIStyle guiStyle;
		[SerializeField] private Rect[] position;


		private Camera m_Camera;
		private bool m_Jump;
		private float m_YRotation;
		private float m_squatposition;
		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private CollisionFlags m_CollisionFlags;
		private bool m_PreviouslyGrounded;
		private Vector3 m_OriginalCameraPosition;
		private float m_StepCycle;
		private float m_NextStep;
		private bool m_Jumping;
		private AudioSource m_AudioSource;
		private float m_speed;
		private bool m_WasSquat;
		private GameObject m_Sparcle;
		private GameObject m_Sparcle1;
		private Vector3 m_bullethitpoint;
		private float m_cooltime;
		private int m_bulletNum;
		private float timer = 0f;
		private GameObject m_enemy;
	

		// Use this for initialization
		private void Start()
		{
			m_CharacterController = GetComponent<CharacterController>();
			m_Camera = Camera.main;
			m_OriginalCameraPosition = m_Camera.transform.localPosition;
			m_FovKick.Setup(m_Camera);
			m_HeadBob.Setup(m_Camera, m_StepInterval);
			m_StepCycle = 0f;
			m_NextStep = m_StepCycle/2f;
			m_Jumping = false;
			m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);
			m_WasSquat = false;
			m_bulletNum = m_bulletLimit;
		}


		// Update is called once per frame
		private void Update()
		{

			timer += Time.deltaTime;

			//ｃボタン押下でしゃがみ機能
			if (Input.GetKeyDown (KeyCode.C)){
				Squat ();
			}

			//	マウスボタン押下による爆発エフェクト生成
			if (Input.GetMouseButtonDown (0) && m_cooltime >= 0.5f && m_bulletNum >= 0) {
				ExplosionEffect ();
			}
				m_cooltime += Time.deltaTime;
			

			//リロード機能
			if (Input.GetKey (KeyCode.R) && m_bulletNum < 30) {
				Reload ();
			}

            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            GetInput(out m_speed);
            // always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
			Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*m_speed;
            m_MoveDir.z = desiredMove.z*m_speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(m_speed);
            UpdateCameraPosition(m_speed);

            m_MouseLook.UpdateCursorLock();
        }
			

        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset() + m_squatposition;
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset() + m_squatposition;
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);

#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
			speed = m_WasSquat ? m_squatspeed : m_WalkSpeed;

            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }

		private void Squat(){
			
				if (!m_WasSquat) {
					m_CharacterController.height = 0.9f;
					m_squatposition = -0.5f;
					m_WasSquat = true;

				} else {
					m_CharacterController.height = 1.8f;
					m_squatposition = 0f;
					m_WasSquat = false;
			}
		}





		private void ExplosionEffect(){
				
				//効果音
				m_AudioSource.PlayOneShot (m_gunSound);	

				//弾数減少
				m_bulletNum --;
				//print (m_bulletBox+","+m_bulletNum);

				//銃口の爆発エフェクト生成
				m_Sparcle  = (GameObject)Instantiate (m_particlePrefab,m_muzzle.transform.position , Quaternion.identity);
				m_Sparcle.transform.parent = m_muzzle.transform;

				//	rayの生成
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;

				//　着弾点を取得
				if (Physics.Raycast (ray, out hit)){

				m_bullethitpoint = hit.point;

				//
				if (hit.collider.tag == "enemy") {
					m_enemy = hit.collider.gameObject;
					score ++;
					m_enemy.transform.GetComponent<Enemy> ().enemyLife--;
				}

				if (hit.collider.tag == "hed") {
					
					float length;
					m_enemy = hit.collider.gameObject;
					length = (m_enemy.transform.position - m_bullethitpoint).magnitude;
					if( 0f < length && length < 0.08f) {
						score += 10;
					} else if (0.08f < length && length < 0.12f) {
						score += 5;
					} else {
						score += 3;
					}
					m_enemy.transform.parent.GetComponent<Enemy> ().enemyLife--;
				}

			}
				//着弾点の爆発エフェクト
				m_Sparcle1 = (GameObject)Instantiate (m_particlePrefab, m_bullethitpoint, Quaternion.identity);	

				//　パーティクル削除	
				Destroy (m_Sparcle, 0.2f);													
				Destroy (m_Sparcle1, 0.2f);							

				//　クールタイムの作成
				m_cooltime = 0f;															
		}




		private void Reload(){

				m_AudioSource.PlayOneShot (m_reloadSound);
				m_bulletBox -= m_bulletLimit;
				m_bulletBox += m_bulletNum;
				m_bulletNum = m_bulletLimit;
		}
		void OnGUI(){

			GUI.Label (position [0], "Time : " + timer, guiStyle);
			GUI.Label (position [1], "Pt : " + score.ToString (), guiStyle);
			GUI.Label (position [2], "BulltBox : " + m_bulletBox, guiStyle);
			GUI.Label (position [3], "Bullet : " + m_bulletNum + "/" + m_bulletLimit, guiStyle);
		}

    }
}
