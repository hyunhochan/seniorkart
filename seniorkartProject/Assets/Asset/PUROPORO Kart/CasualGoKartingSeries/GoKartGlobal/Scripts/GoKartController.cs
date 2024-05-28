using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


namespace PUROPORO
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    public class GoKartController : NetworkBehaviour
    {
        public struct KartInput : INetworkSerializable
        {
            public bool Acceleration;
            public bool DeAcceleration;
            public bool LeftSteering;
            public bool RightSteering;
            public bool IsBraking;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Acceleration);
                serializer.SerializeValue(ref DeAcceleration);
                serializer.SerializeValue(ref LeftSteering);
                serializer.SerializeValue(ref RightSteering);
                serializer.SerializeValue(ref IsBraking);
            }
        }

        public struct DriveFactor
        {
            public float speedLimit;
            public float frontGripFactor;
            public float rearGripFactor;
            public float driftSlipFactor;
            public float backFrontGripFactor;
            public float backRearGripFactor;
            public float backDriftSlipFactor;
            public float betaCut;
            public float onTriggerSteerFactor;
            public float onDriftSteerFactor;
            public float onRestTimeSteerFactor;

            public void Initialize()
            {
                frontGripFactor = 0.0f;
                rearGripFactor = 0.0f;
                driftSlipFactor = 1f;
                backFrontGripFactor = 0.0f;
                backRearGripFactor = 0.0f;
                backDriftSlipFactor = 1f;
                betaCut = 1f;
                onDriftSteerFactor = 1f;
                onRestTimeSteerFactor = 1f;
                onTriggerSteerFactor = 1f;
                speedLimit = 120f;
            }
        }

        public struct PhysicSpec
        {
            public float airFriction;
            public float dragFactor;
            public float forwardAccel;
            public float backwardAccel;
            public float gripBrake;
            public float slipBrake;
            public float maxSteerDeg;
            public float steerConstraint;
            public float frontGripFactor;
            public float rearGripFactor;
            public float driftTrigFactor;
            public float driftTrigTime;
            public float driftSlipFactor;
            public float driftEscapeForce;
            public float cornerDrawFactor;
            public float driftLeanFactor;
            public float steerLeanFactor;
            public float driftMaxGauge;
            public float mass;
            public float width;
            public float length;
            public float springK;
            public float damperCopC;
            public float damperRebC;

            public void Initialize()
            {
                mass = 100f;
                airFriction = 3.0f;
                dragFactor = 0.656f;
                forwardAccel = 2100.0f;
                backwardAccel = 1600.0f;
                gripBrake = 1800.0f;
                slipBrake = 1250.0f;
                maxSteerDeg = 10.0f;
                steerConstraint = 24.7f;
                frontGripFactor = 5.0f;
                rearGripFactor = 5.0f;
                driftTrigFactor = 0.2f;
                driftTrigTime = 0.2f;
                driftSlipFactor = 0.2f;
                driftEscapeForce = 3850.0f;
                cornerDrawFactor = 0.215f;
                driftLeanFactor = 0.06f;
                steerLeanFactor = 0.01f;
                driftMaxGauge = 4000.0f;
            }

            public float getMaxSteerRad() => (float)(3.1415927410125732 * (double)maxSteerDeg / 180.0);
        }

        public struct FirstPipelineValue
        {
            public Vector3 left;
            public Vector3 up;
            public Vector3 front;
            public float frontVel;
            public float leftVel;
            public float upVel;
            public float speed;
        }

        public struct Suspension
        {
            public Vector2[] wheelOff;
            public Vector3 contactN;
            public bool[] wheelContact;
            public Vector3[] wheelContactN;
            public float maxTravel;
            public float[] travel;
            public float[] deltaTravel;

            public void InitVector2(ref Vector2 v, float x, float y)
            {
                v.x = x;
                v.y = y;
            }

            public void Initialize()
            {
                wheelOff = new Vector2[4];
                for (int index = 0; index < 4; ++index)
                    InitVector2(ref wheelOff[index], index % 2 != 0 ? 1f : -1f, index / 2 != 0 ? -1f : 1f);

                wheelContact = new bool[4];
                wheelContactN = new Vector3[4];
                maxTravel = 0.5f;
                travel = new float[4] { 0.5f, 0.5f, 0.5f, 0.5f };
                deltaTravel = new float[4];
            }
        }

        public struct DriftControl
        {
            public bool slipMode;
            public float slipTime;
            public bool forceSlip;
            public bool trigger;
            public float triggerTime;

            public void Initialize()
            {
                slipMode = false;
                slipTime = 0.0f;
                forceSlip = false;
                trigger = false;
                triggerTime = 0.0f;
            }
        }

        public struct Control
        {
            public float accel;
            public float brake;
            public bool accelBrakeSwap;
            public float steer;
            public bool wheelFlip;
            public bool wheelDevil;
            public float stayTime;
            public float steerAngle;
            public float oldSteerAngle;

            public float getRealAccel() => accelBrakeSwap ? brake : accel;

            public float getRealBrake() => accelBrakeSwap ? accel : brake;

            public float getRealSteer() => (wheelFlip || wheelDevil ? -1f : 1f) * steer;

            public void Initialize()
            {
                accel = 0.0f;
                brake = 0.0f;
                accelBrakeSwap = false;
                steer = 0.0f;
                wheelFlip = false;
                wheelDevil = false;
                stayTime = 0.0f;
                steerAngle = 0.0f;
                oldSteerAngle = 0.0f;
            }
        }

        public struct CollisionState
        {
            public bool kartCollide;
            public float kartCollideVel;
            public bool kartCollideDominant;
            public bool shock;
            public float shockVel;
            public bool hop;
            public float unmovingTime;

            public void Initialize()
            {
                kartCollide = false;
                kartCollideVel = 0.0f;
                kartCollideDominant = false;
                shock = false;
                hop = false;
                unmovingTime = 0.0f;
            }
        }

        public struct External
        {
            public bool slip;
            public float dragFactor;
            public float compensationDragFactor;
            public float wheelFactor;
            public Vector3 annexForce;
            public Vector3 force;
            public Vector3 torque;
            public float gravityFactor;
            public float speedLimit;
            public float upDownTime;
            public float upDownLastTime;
            public float upDownInterval;
            public Vector3 upDownForce;
            public uint upDownForceIndex;
            public Vector3 liftVel;

            public void Initialize()
            {
                slip = false;
                dragFactor = 1f;
                compensationDragFactor = 1f;
                wheelFactor = 1f;
                annexForce = Vector3.zero;
                force = Vector3.zero;
                torque = Vector3.zero;
                upDownTime = 0.0f;
                upDownLastTime = 0.0f;
                gravityFactor = 1f;
                speedLimit = 0.0f;
            }
        }

        public struct StuckHelper
        {
            public float wallStuckTime;
            public float gndStuckTime;
            public float obstStuckTime;
            public bool inStuck;

            public void Initialize()
            {
                wallStuckTime = 0.0f;
                obstStuckTime = 0.0f;
                inStuck = false;
            }
        }

        public Vector3 m_KartWLVel;
        public Vector3 m_KartLAVel;
        public Vector3 m_KartRealVelocity;
        public bool stuck_;
        private Matrix3 m_ort;
        private Vector3 m_reciprocalMass;
        private Vector3 m_theGravity;
        public Vector3 m_NetWForce;
        private Vector3 m_NetLTorque;
        private int m_boostLeft;
        private float m_slipReserveTime;
        private float m_steer;
        private float m_grip;
        private float[] m_slip = new float[2];
        private bool m_slipBoost;
        public PhysicSpec m_spec;
        public FirstPipelineValue m_first;
        public Suspension m_sus;
        public DriftControl m_drift;
        public Control m_ctrl;
        public CollisionState m_cState;
        private External m_extern;
        private DriveFactor[,] m_DriveFactor;
        private StuckHelper m_stuckHelper;
        public bool m_Contact;
        public bool m_isDrift;
        private uint m_DriveMode;
        public Vector3[] wheelLocalPos_;
        public bool isCrash_;
        public float crashVelocity_;
        public bool isShock_;
        public float shockVelocity_;
        private bool needReset_;
        private bool _steerLeft;
        private bool _steerRight;
        private bool _driftPressed;
        private bool _lastPhysicsFrame;
        public bool[] isBackupStreer;
        private Vector3 _targetUpVector = Vector3.up;
        private bool grounded;
        private PrevState prevState_;

        public bool inputAcceleration;
        public bool inputDeAcceleration;
        public bool inputLeftSteering;
        public bool inputRightSteering;
        public bool isBraking;
        //[HideInInspector] public float inputAcceleration;
        //[HideInInspector] public float inputSteering;
        //[HideInInspector] public bool isBraking;

        public KartCheckpoint kartcheckpoint;

        public class PrevState
        {
            public Vector3 position_ = Vector3.zero;
            public Vector3 force_ = Vector3.zero;
            public Vector3 velocity_ = Vector3.zero;
            public Vector3 angular_ = Vector3.zero;
            public Vector3 forward_ = Vector3.zero;
            public Quaternion rotate_;
            public bool isGrounded_;
        }

        [Header("Camera")]
        public GameObject cameraMountPoint;
        private Camera playerCamera;
        private NetworkTransform networkTransform;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = false;
            rb.useGravity = false;
            rb.isKinematic = true;
            ResetPhysicsValues();
        }

        private void Start()
        {
            ResetPhysicsValues();
            networkTransform = GetComponent<NetworkTransform>();
            if (IsOwner)
            {
                SetupCamera();
            }
            InitializeRigidbodyServerRpc();
        }

        [ServerRpc]
        private void InitializeRigidbodyServerRpc()
        {
            InitializeRigidbodyClientRpc();
        }

        [ClientRpc]
        private void InitializeRigidbodyClientRpc()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            ResetPhysicsValues();
        }

        private void SendInputToServer()
        {
            var input = new KartInput
            {
                Acceleration = inputAcceleration,
                DeAcceleration = inputDeAcceleration,
                LeftSteering = inputLeftSteering,
                RightSteering = inputRightSteering,
                IsBraking = isBraking
            };

            SubmitInputServerRpc(input);
        }

        [ServerRpc]
        private void SubmitInputServerRpc(KartInput input)
        {
            if (!IsOwner)
            {
                if (input.Acceleration)
                {
                    setAccel(true);
                }
                else if (!input.Acceleration)
                {
                    setAccel(false);
                }

                if (input.DeAcceleration)
                {
                    setBrake(true);
                }
                else if (!input.DeAcceleration)
                {
                    setBrake(false);
                }

                if (input.LeftSteering)
                {
                    setSteer(-1.0f);
                    _steerLeft = true;
                    updateDriftPressed(true);
                }
                else if (!input.LeftSteering)
                {
                    _steerLeft = false;
                    setSteer(_steerLeft || _steerRight ? _steerLeft ? -1.0f : 1.0f : 0.0f);
                    updateDriftPressed(false);
                }

                if (input.RightSteering)
                {
                    setSteer(1.0f);
                    _steerRight = true;
                    updateDriftPressed(true);
                }
                else if (!input.RightSteering)
                {
                    _steerRight = false;
                    setSteer(_steerLeft || _steerRight ? _steerLeft ? -1.0f : 1.0f : 0.0f);
                    updateDriftPressed(false);
                }

                if (input.IsBraking)
                {
                    _driftPressed = true;
                    updateDriftPressed(true);
                }
                else if (!input.IsBraking)
                {
                    _driftPressed = false;
                    updateDriftPressed(false);
                }
                // 서버에서 입력을 처리하고 클라이언트의 상태를 보정
                //HandleAcceleration(input.Acceleration);
                //HandleSteering(input.Steering);
                //HandleBraking(input.IsBraking);
            }
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.enabled = false;
            }

            playerCamera = new GameObject("PlayerCamera").AddComponent<Camera>();
            playerCamera.transform.SetParent(cameraMountPoint.transform);
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;

            var cameraFollow = playerCamera.gameObject.AddComponent<CameraFollowBehind>();
            cameraFollow.SetTarget(cameraMountPoint.transform);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                setAccel(true);
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                setAccel(false);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                setBrake(true);
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                setBrake(false);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                setSteer(-1.0f);
                _steerLeft = true;
                updateDriftPressed(true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                _steerLeft = false;
                setSteer(_steerLeft || _steerRight ? _steerLeft ? -1.0f : 1.0f : 0.0f);
                updateDriftPressed(false);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                setSteer(1.0f);
                _steerRight = true;
                updateDriftPressed(true);
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                _steerRight = false;
                setSteer(_steerLeft || _steerRight ? _steerLeft ? -1.0f : 1.0f : 0.0f);
                updateDriftPressed(false);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _driftPressed = true;
                updateDriftPressed(true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                _driftPressed = false;
                updateDriftPressed(false);
            }
        }

        private void updateDriftPressed(bool pressed)
        {
            if (!pressed)
            {
                setDrift(false);
            }
            else if (_driftPressed && m_ctrl.steer != 0)
            {
                setDrift(true);
            }
        }


        public void setAccel(bool accel)
        {
            Debug.Log("setAccel accel=" + accel);

            if (accel)
            {
                m_ctrl.accel = 1.0f;
                m_slipBoost = true;
            }
            else
            {
                m_ctrl.accel = 0.0f;
            }
        }

        public void setBrake(bool brake)
        {
            Debug.Log("setBrake brake=" + brake);

            if (brake)
            {
                m_ctrl.brake = 1.0f;
            }
            else
            {
                m_ctrl.brake = 0.0f;
            }
        }

        public void setDrift(bool drift)
        {
            Debug.Log("setDrift drift=" + drift);

            if (drift)
            {
                if (m_drift.slipTime <= 0.0f)
                {
                    m_drift.slipMode = true;
                    m_drift.trigger = true;
                }
            }
            else
            {
                m_drift.slipMode = false;
            }
        }

        public void setSteer(float value)
        {
            Debug.Log("setSteer steer=" + value);

            m_ctrl.steer = value;
        }

        public void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c)
        {
            // create matrix
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(pos, rot, scale);

            var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
            var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
            var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
            var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

            var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
            var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
            var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
            var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

            Debug.DrawLine(point1, point2, c);
            Debug.DrawLine(point2, point3, c);
            Debug.DrawLine(point3, point4, c);
            Debug.DrawLine(point4, point1, c);

            Debug.DrawLine(point5, point6, c);
            Debug.DrawLine(point6, point7, c);
            Debug.DrawLine(point7, point8, c);
            Debug.DrawLine(point8, point5, c);

            Debug.DrawLine(point1, point5, c);
            Debug.DrawLine(point2, point6, c);
            Debug.DrawLine(point3, point7, c);
            Debug.DrawLine(point4, point8, c);
        }

        void FixedUpdate()
        {
            if (IsOwner)
            {
                GetInput();
                m_isDrift = false;
                float fixedDeltaTime = Time.fixedDeltaTime;

                beginNetForce(fixedDeltaTime);
                if (decideKartContact(fixedDeltaTime))
                {
                    calcNonpenetrateForce(fixedDeltaTime);
                    calcKartTractionForce(fixedDeltaTime);
                    calcKartSteeringForce(fixedDeltaTime);
                }
                else
                {
                    calcFlyingKartForce();
                }
                calcResistForce();
                endNetForce(fixedDeltaTime);

                m_isDrift = m_isDrift || m_drift.slipMode || m_drift.slipTime > 0f || m_drift.forceSlip;


                Vector3 targetUpVector = _targetUpVector;
                Vector3 right = base.transform.right;
                Vector3 forward = base.transform.forward;
                forward = Vector3.Cross(right, targetUpVector);
                float num = (!grounded) ? 1f : 10f;
                Quaternion to = Quaternion.LookRotation(forward, targetUpVector);
                Quaternion localRotation = Quaternion.Slerp(base.transform.localRotation, to, Time.deltaTime * num);
                base.transform.localRotation = localRotation;
                _targetUpVector = Vector3.up;
                bool flag8 = grounded;
                if (flag8)
                {
                    m_KartLAVel.x = 0f;
                    m_KartLAVel.z = 0f;
                    Quaternion localRotation2 = base.transform.localRotation;
                    Quaternion rhs = MathHelper.CreateQuaternion(0f, m_KartLAVel);
                    Quaternion q = localRotation2 * rhs;
                    MathHelper.QuaMulScala(ref q, 0.5f * Time.deltaTime);
                    MathHelper.QuaAdd(ref localRotation2, q);
                    MathHelper.QuaNormalize(ref localRotation2);
                    base.transform.localRotation = localRotation2;
                    prevState_.rotate_ = localRotation2;
                }
                Vector3 velocity_ = prevState_.velocity_;
                Vector3 vector = m_KartWLVel - velocity_;
                vector.x = Mathf.Clamp(vector.x, -100f, 100f);
                vector.z = Mathf.Clamp(vector.z, -100f, 100f);
                vector.y = 0f;
                Vector3 kartWLVel = m_KartWLVel;
                Vector3 b = kartWLVel * Time.deltaTime;
                Vector3 position = base.transform.position;
                Vector3 vector2 = base.transform.position + b;
                Vector3 vector3 = vector2 - position;
                Bounds bounds = GetComponent<Rigidbody>().GetComponent<Collider>().bounds;
                float num2 = Mathf.Min(new float[]
                {
                bounds.size.x,
                bounds.size.y,
                bounds.size.z
                });
                bool flag9 = vector3.magnitude >= num2 / 2f;
                if (flag9)
                {
                    Vector3 center = bounds.center;
                    Vector3 vector4 = bounds.center + b;
                    vector4 = GetCollisionCheckedPosition(center, vector4);
                    vector2 = base.transform.position + (vector4 - center);
                }
                base.transform.position = vector2;


                Vector3 a = GetComponent<Rigidbody>().position - prevState_.position_;
                m_KartRealVelocity = a / Time.deltaTime;
                prevState_.position_ = GetComponent<Rigidbody>().position;
                prevState_.velocity_ = m_KartWLVel;
                prevState_.angular_ = GetComponent<Rigidbody>().angularVelocity;
                prevState_.forward_ = transform.forward;
                prevState_.isGrounded_ = grounded;
                grounded = false;

                SendInputToServer();
            }
        }

        private void GetInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                inputAcceleration = true;
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                inputAcceleration = false;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                inputDeAcceleration = true;
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                inputDeAcceleration = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                inputLeftSteering = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                inputLeftSteering = false;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                inputRightSteering = true;
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                inputRightSteering = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isBraking = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                isBraking = false;
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Checkpoint"))
            {
                Physics.IgnoreCollision(collider, GetComponent<Collider>(), true);
                
                return;
            }
            else
            {
                
                UpdateTriggerPosition(collider, true);
            }
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.CompareTag("Checkpoint"))
            {
                
                return;
            }
            else
            {
                
                UpdateTriggerPosition(collider, false);
            }
        }



private void UpdateTriggerPosition(Collider collider, bool firstTrigger)
{
    float num = 0.65f;
    Vector3 position = base.transform.position;
    Vector3 vector = -base.transform.up;
    Vector3 origin = position - vector;
    float magnitude = vector.magnitude;
    Ray ray = new Ray(origin, vector);
    RaycastHit[] array = Physics.RaycastAll(ray, magnitude * 1f);
    if (array != null && array.Length != 0)
    {
        foreach (RaycastHit hit in array)
        {
            if (hit.collider.CompareTag("Checkpoint"))
            {
                continue; // "Checkpoint" 태그를 가진 Collider는 무시
            }

            // 기존 충돌 처리 코드
            bool flag4 = false;
            float num2 = hit.distance;
            Vector3 vector2 = hit.normal;
            int num4 = hit.collider.gameObject.layer;
            flag4 = true;

            if (flag4)
            {
                bool flag7 = magnitude - num2 > 0f;
                if (flag7)
                {
                    _targetUpVector = vector2;
                    flag7 = true;
                }

                if (flag7)
                {
                    float num5 = 1 << num4;
                    bool flag10 = magnitude - num2 > 0f && num5 != 16384f;
                    if (flag10)
                    {
                        Vector3 b = -vector * (magnitude - num2);
                        base.transform.position += b;
                    }
                    float num6 = Vector3.Dot(m_KartWLVel, vector2);
                    Vector3 vector3 = vector2 * num6;
                    Vector3 b2 = m_KartWLVel - vector3;
                    bool flag11 = vector2.y > num;
                    if (flag11)
                    {
                        float num7 = Mathf.Abs(Vector3.Dot(m_first.front, vector2)) * 0.7f;
                        m_KartWLVel = vector3 * -num7 + b2;
                        shockVelocity_ = vector3.magnitude;
                        grounded = true;
                        bool flag12 = !prevState_.isGrounded_;
                        if (flag12)
                        {
                            isShock_ = true;
                            var shockVel = Mathf.Abs(num6);
                            if (shockVelocity_ <= shockVel)
                            {
                                shockVelocity_ = shockVel;
                            }
                        }
                    }
                    else
                    {
                        bool isCrash_ = this.isCrash_;
                        if (isCrash_)
                        {
                            float num8 = Mathf.Abs(num6);
                            this.isCrash_ = true;
                            if (crashVelocity_ <= num8)
                            {
                                crashVelocity_ = num8;
                            }
                        }
                    }
                    float num9 = Vector3.Dot(Vector3.Cross(vector2, Vector3.up), m_first.front);
                    base.transform.RotateAroundLocal(base.transform.up, num9 * 0.3f * Time.deltaTime);
                }
            }
        }
    }

    // HandleCollision 호출 부분에서도 수정 필요
    bool flag16 = true;
    if (flag16)
    {
        Vector3 center = GetComponent<Collider>().bounds.center;
        Vector3 size = GetComponent<Collider>().bounds.size;
        HandleCollision(base.transform.right, center, size.x / 2f, true, num);
        HandleCollision(-base.transform.right, center, size.x / 2f, true, num);
        HandleCollision(base.transform.forward, center, size.z * 0.75f, true, num);
        HandleCollision(-base.transform.forward, center, size.z / 2f, true, num);
    }
}



        private bool HandleCollision(Vector3 localRayDirection, Vector3 rayOrigin, float rayLength, bool handleCrash, float gndLimit)
{
    RaycastHit raycastHit;
    bool flag = Physics.Raycast(rayOrigin, localRayDirection, out raycastHit, rayLength);
    if (flag && !raycastHit.collider.CompareTag("Checkpoint")) // "Checkpoint" 태그 확인
    {
        Vector3 vector = raycastHit.normal;
        bool flag2 = raycastHit.distance < rayLength && vector.y < gndLimit;
        if (flag2)
        {
            float num = Vector3.Dot(vector, localRayDirection);
            bool flag3 = num > 0f;
            if (flag3)
            {
                vector = -vector;
            }
            float d = Mathf.Abs(num);
            float num2 = 1 << raycastHit.collider.gameObject.layer;
            bool flag4 = num2 != 16384f;
            if (flag4)
            {
                Vector3 b = -vector * (raycastHit.distance - rayLength) * d;
                base.transform.position += b;
            }
            bool flag5 = handleCrash && !isCrash_;
            if (flag5)
            {
                float num3 = Vector3.Dot(vector, prevState_.velocity_);
                bool flag6 = num3 < 0f;
                if (flag6)
                {
                    float num4 = Mathf.Abs(num3);
                    isCrash_ = true;
                    if (crashVelocity_ <= num4)
                    {
                        crashVelocity_ = num4;
                    }

                    m_ctrl.oldSteerAngle = 0f;
                    float d2 = 1.618f;
                    Vector3 a = Vector3.zero;
                    bool flag8 = raycastHit.collider.gameObject.transform.parent != null;
                    if (flag8)
                    {
                        a = m_KartWLVel;
                    }

                    Vector3 lhs = m_KartWLVel - a * d2;
                    Vector3 vector2 = vector * Vector3.Dot(lhs, vector);
                    Vector3 b2 = m_KartWLVel - vector2;
                    bool flag11 = vector.y > gndLimit;
                    if (flag11)
                    {
                        float num5 = Mathf.Abs(Vector3.Dot(m_first.front, vector)) * 0.7f;
                        m_KartWLVel = vector2 * -num5 + b2;
                        m_cState.shockVel = vector2.magnitude;
                    }
                    else
                    {
                        float magnitude = b2.magnitude;
                        Vector3 a2 = (magnitude <= 0f) ? Vector3.zero : b2.normalized;
                        Vector3 vector3 = Vector3.zero;
                        vector3 = vector2 * -1.5f - a2 * Mathf.Min(vector2.magnitude * 1.5f, magnitude * 0.6f);
                        bool flag12 = !m_drift.slipMode && m_Contact;
                        if (flag12)
                        {
                            bool flag13 = false;
                            float num6 = 0f;
                            float d3 = 0f;
                            bool flag14 = Mathf.Abs(Vector3.Dot(vector, base.transform.forward)) > 0.5f && Mathf.Approximately(m_ctrl.getRealAccel(), 1f);
                            if (flag14)
                            {
                                bool flag15 = Mathf.Approximately(m_ctrl.steer, 1f);
                                if (flag15)
                                {
                                    flag13 = true;
                                    num6 = 1f;
                                    d3 = 1f;
                                }
                                else
                                {
                                    bool flag16 = Mathf.Approximately(m_ctrl.steer, -1f);
                                    if (flag16)
                                    {
                                        flag13 = true;
                                        num6 = -1f;
                                        d3 = -1f;
                                    }
                                }
                            }
                            bool flag17 = flag13;
                            if (flag17)
                            {
                                Vector3 lhs2 = vector;
                                Vector3 rhs = -m_first.front;
                                rhs.Normalize();
                                float num7 = Vector3.Dot(lhs2, rhs);
                                bool flag18 = Vector3.Dot(lhs2, m_first.left * d3) < 0f;
                                float num8;
                                if (flag18)
                                {
                                    num7 = 2f - num7;
                                    num8 = Mathf.Max(num7, 1.5f);
                                }
                                else
                                {
                                    num8 = num7 * num7 * num7;
                                }
                                vector3 -= vector2.normalized * (3f * num7 + 1f);
                                vector3 += m_first.left * (3f * num7 + 1f) * num6;
                                float angle = (6f * num8 + 5f) * num6 * Time.deltaTime;
                                base.transform.RotateAroundLocal(Vector3.up, angle);
                            }
                        }
                        m_KartWLVel += vector3;
                        float num9 = Vector3.Dot(vector, m_first.front);
                        float num10 = Vector3.Dot(vector, m_first.left);
                        float num11 = 3f;
                        bool flag19 = Mathf.Abs(num9 * 0.8f) > Mathf.Abs(num10);
                        if (flag19)
                        {
                            Vector3 zero = Vector3.zero;
                            zero.z = num10 * ((num9 <= 0f) ? -1f : 1f) * Mathf.Max(1f, Mathf.Min(30f, Mathf.Abs(Vector3.Dot(vector, m_KartWLVel) * 0.5f)));
                            bool flag20 = Vector3.Dot(zero, m_KartLAVel) <= 1f;
                            if (flag20)
                            {
                                float angle2 = zero.z * Time.deltaTime * num11;
                                base.transform.RotateAroundLocal(Vector3.up, angle2);
                            }
                        }
                        else
                        {
                            Vector3 zero2 = Vector3.zero;
                            zero2.z = num9 * ((num10 <= 0f) ? 1f : -1f) * Mathf.Max(1f, Mathf.Min(30f, Mathf.Abs(Vector3.Dot(vector, m_KartWLVel) * 0.5f)));
                            bool flag21 = Vector3.Dot(zero2, m_KartLAVel) <= 1f;
                            if (flag21)
                            {
                                float angle3 = zero2.z * Time.deltaTime * num11;
                                base.transform.RotateAroundLocal(Vector3.up, angle3);
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
    return false;
}




        private Vector3 GetCollisionCheckedPosition(Vector3 fromPosition, Vector3 toPosition)
        {
            Vector3 direction = toPosition - fromPosition;
            Vector3 normalized = direction.normalized;
            float magnitude = direction.magnitude;
            RaycastHit[] array = Physics.RaycastAll(fromPosition, direction, direction.magnitude);
            bool flag = array != null && array.Length != 0;
            if (flag)
            {
                float num = magnitude;
                foreach (RaycastHit raycastHit in array)
                {
                    bool flag2 = !raycastHit.collider.gameObject.Equals(base.gameObject) && raycastHit.distance < (double)num;
                    if (flag2)
                    {
                        toPosition = fromPosition + normalized * (raycastHit.distance - 0.1f);
                        num = raycastHit.distance;
                    }
                }
            }
            return toPosition;
        }

        private Vector3 _lastCollisionPos;
        private bool applyCollision(float deltaT, float wallZ)
        {
            bool result = false;
            Vector3 size = new Vector3(m_spec.width, 0.7f, m_spec.length);
            var rot = transform.localRotation;
            Vector3 top = transform.position + m_first.up;

            DrawBox(top, rot, size, Color.red);

            var deltaPos = transform.position - _lastCollisionPos;
            _lastCollisionPos = transform.position;


            var hits = Physics.BoxCastAll(top, size, deltaPos.normalized, rot, deltaPos.magnitude);
            foreach (var hit in hits)
            {
                // hack fix
                if (hit.collider.name.StartsWith("CheckPoint"))
                    continue;
                if (hit.collider.CompareTag("Checkpoint"))
                    continue;

                var contactPoint = hit.point;
                var normal = hit.normal;

                {
                    /*
                    Debug.Log( "Collision" );
                    Debug.Log( hit.normal );
                    Debug.Log( hit.point );
                    Debug.Log( hit.collider?.gameObject?.name );
                    */

                    // Vector3 normal = hit.normal;
                    float dot = Vector3.Dot(m_KartWLVel, normal);
                    if (dot < 0.0f)
                    {
                        m_ctrl.oldSteerAngle = 0.0f;
                        result = true;

                        Vector3 shock = normal * Vector3.Dot(m_KartWLVel, normal);
                        Vector3 shockOffset = m_KartWLVel - shock;

                        if (normal.y > wallZ)
                        {
                            m_KartWLVel = shock * -(Mathf.Abs(Vector3.Dot(m_first.front, normal)) * 0.7f) + shockOffset;
                            m_cState.shockVel = shock.magnitude;
                            m_KartLAVel.x -= Vector3.Dot(Vector3.Cross(normal, m_first.up), m_first.left) * 0.1f;
                            m_KartLAVel.z -= Vector3.Dot(Vector3.Cross(normal, m_first.up), -m_first.front) * 0.1f;
                        }
                        else
                        {
                            float shockDist = shockOffset.magnitude;
                            Vector3 addSub = shockDist > 0 ? shockOffset.normalized : Vector3.zero;
                            Vector3 add = shock * -1.5f - addSub * Math.Min(shock.magnitude * 1.5f, shockDist * 0.6f);
                            add -= new Vector3(0, 1, 0) * add.y;
                            if (m_KartWLVel.sqrMagnitude < 100 && !m_drift.slipMode && m_Contact)
                            {
                                bool crash = false;
                                float crashDir = 0;
                                float crashDir2 = 0;
                                if (m_ctrl.getRealAccel() == 1)
                                {
                                    if (m_ctrl.steer == 1)
                                    {
                                        crash = true;
                                        crashDir = m_ctrl.wheelFlip ? -1.0f : 1.0f;
                                        crashDir2 = 1;
                                    }
                                    else if (m_ctrl.steer == -1)
                                    {
                                        crash = true;
                                        crashDir = m_ctrl.wheelFlip ? 1.0f : -1.0f;
                                        crashDir2 = -1;
                                    }
                                }
                                if (crash)
                                {
                                    Vector3 up = normal;
                                    Vector3 back = -m_first.front;
                                    back.Normalize();
                                    float crashMin = Vector3.Dot(up, back);
                                    float crashVel = 0.0f;
                                    Vector3 left = m_first.left * crashDir2;
                                    if (Vector3.Dot(up, left) < 0)
                                    {
                                        crashMin = 2 - crashMin;
                                        crashVel = Mathf.Max(crashMin, 1.5f);
                                    }
                                    else
                                    {
                                        crashVel = crashMin * crashMin * crashMin;
                                    }
                                    add -= shock.normalized * (3 * crashMin + 1);
                                    add += m_first.left * (3 * crashMin + 1) * crashDir;
                                    m_KartLAVel.y += (6 * crashVel + 2) * crashDir;
                                    // m_driftOrCrash2 = true; // m_sound.driftOrCrash2 = true;
                                    // m_crash2Vel = max( crashMin, 1.0f ) * 0.3f + 0.1f;
                                }
                            }
                            m_KartWLVel += add;

                            float dotFront = Vector3.Dot(normal, m_first.front);
                            float dotLeft = Vector3.Dot(normal, m_first.left);
                            if (Mathf.Abs(dotLeft) < Mathf.Abs(dotFront * 0.8f))
                            {
                                add = Vector3.zero;
                                float dir = dotFront > 0 ? 1.0f : -1.0f;
                                float addValue = Mathf.Abs(Vector3.Dot(normal, m_KartWLVel) * 0.5f);
                                add.y = dotLeft * dir * Mathf.Clamp(addValue, 1.0f, 30.0f);
                                if (Vector3.Dot(add, m_KartLAVel) <= 1)
                                    m_KartLAVel += add;
                            }
                            else
                            {
                                add = Vector3.zero;
                                float dir = dotLeft > 0 ? -1.0f : 1.0f;
                                float addValue = Mathf.Abs(Vector3.Dot(normal, m_KartWLVel) * 0.5f);
                                add.y = dotFront * dir * Mathf.Clamp(addValue, 1.0f, 30.0f);
                                if (Vector3.Dot(add, m_KartLAVel) <= 1)
                                    m_KartLAVel += add;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private void calcResistForce()
        {
            Vector3 force = Vector3.zero;
            Vector3 torque = Vector3.zero;
            force -= m_KartWLVel * m_spec.airFriction;
            torque -= m_KartLAVel * m_spec.airFriction;

            if (m_Contact)
                force -= m_KartWLVel
                    * m_KartWLVel.magnitude
                    * m_spec.dragFactor
                    * m_extern.dragFactor
                    * m_extern.compensationDragFactor;

            m_NetWForce += force;
            m_NetLTorque += torque;
        }


        private void calcKartSteeringForce(float deltaT)
        {

            if (!m_extern.slip)
            {
                float num = Mathf.Sqrt(m_first.frontVel * m_first.frontVel + m_first.leftVel * m_first.leftVel);
                float num2 = (m_first.frontVel <= 0f) ? -1f : 1f;
                m_ctrl.steerAngle = m_ctrl.getRealSteer() * m_spec.getMaxSteerRad();
                m_ctrl.steerAngle = m_ctrl.steerAngle * Mathf.Exp(-Mathf.Abs(m_first.frontVel / m_spec.steerConstraint * m_extern.wheelFactor));
                isBackupStreer[0] = m_ctrl.getRealAccel() != 0f;
                isBackupStreer[1] = m_ctrl.oldSteerAngle * m_ctrl.steerAngle > 0f;
                isBackupStreer[2] = Mathf.Abs(m_ctrl.oldSteerAngle) < Mathf.Abs(m_ctrl.steerAngle);
                if (isBackupStreer[0] && isBackupStreer[1] && isBackupStreer[2])
                {
                    m_ctrl.steerAngle = m_ctrl.oldSteerAngle;
                }
                else
                {
                    m_ctrl.oldSteerAngle = m_ctrl.steerAngle;
                }
                float num3 = 0.5f;
                float num4 = 0.5f;
                bool flag = false;
                uint driveMode = 0U;
                bool flag2 = m_drift.slipMode || m_drift.forceSlip;
                m_drift.forceSlip = false;
                if (num > 5f)
                {
                    float num5 = m_KartLAVel.y * num3 / num;
                    float num6 = m_KartLAVel.y * num4 / num;
                    float num7 = m_first.leftVel / num;
                    bool flag3 = false;

                    if (!m_drift.slipMode && !m_drift.trigger && Mathf.Abs(m_first.leftVel) > Mathf.Abs(m_first.frontVel) * 1.2f && num > 15f)
                    {
                        m_drift.forceSlip = true;
                    }
                    float z = 0f;
                    float num8;
                    float num9;
                    if (m_drift.trigger)
                    {
                        m_steer = 0f;
                        m_grip = 0f;
                        num8 = 0f;
                        num9 = -(9.8f * m_spec.mass) * m_spec.frontGripFactor * (m_ctrl.getRealSteer() * m_spec.getMaxSteerRad() * m_spec.driftTrigFactor);
                        if (m_drift.triggerTime <= 0f)
                        {
                            m_drift.triggerTime = m_spec.driftTrigTime;
                            m_drift.slipTime = m_drift.triggerTime * 2f;
                        }
                        else
                        {
                            m_drift.triggerTime = m_drift.triggerTime - deltaT;
                            if (m_drift.triggerTime <= 0f)
                            {
                                m_drift.triggerTime = 0f;
                                m_drift.trigger = false;
                            }
                        }
                    }
                    else if (m_drift.slipMode || m_drift.slipTime > 0f || m_drift.forceSlip || m_slipReserveTime > 0f)
                    {
                        if (m_DriveMode == 1U)
                        {
                            float num10 = num / m_DriveFactor[m_DriveMode, 1].speedLimit;
                            float num10Sq = num10 * num10;
                            if (num10Sq > 1f)
                                num10Sq = 1f;
                            float onDriftSteer;
                            float frontGrip;
                            float rearGrip;
                            if (m_drift.slipMode)
                            {
                                m_steer += 1E-06f * (1f - m_steer) * num10Sq;
                                m_grip += 0.005f * (1f - m_grip);
                                m_NetWForce *= 1f - m_grip;
                                onDriftSteer = m_ctrl.getRealSteer() * m_spec.getMaxSteerRad() * m_DriveFactor[m_DriveMode, 1].onDriftSteerFactor;
                                frontGrip = m_DriveFactor[m_DriveMode, 1].frontGripFactor;
                                rearGrip = m_DriveFactor[m_DriveMode, 1].rearGripFactor;
                            }
                            else
                            {
                                m_steer += 0.001f * (1f - m_steer);
                                m_grip = 0f;
                                onDriftSteer = m_ctrl.steerAngle * m_DriveFactor[m_DriveMode, 1].onRestTimeSteerFactor;
                                frontGrip = m_DriveFactor[m_DriveMode, 1].backFrontGripFactor;
                                rearGrip = m_DriveFactor[m_DriveMode, 1].backRearGripFactor;
                            }
                            float num15 = m_spec.frontGripFactor + frontGrip;
                            float num16 = m_spec.rearGripFactor + rearGrip;
                            if (m_steer > 1f)
                            {
                                m_steer = 1f;
                            }
                            num16 -= m_grip;
                            num8 = 9.8f * m_spec.mass * num15 * (onDriftSteer * num2 - num7 - num5);
                            num9 = 9.8f * m_spec.mass * num16 * (-num7 + num6);
                            num8 *= m_spec.driftSlipFactor * m_steer;
                            num9 *= m_spec.driftSlipFactor * m_steer;
                            m_slipReserveTime = Mathf.Max(m_slipReserveTime - deltaT, 0f);
                        }
                        else if (m_drift.slipMode)
                        {
                            num8 = 9.8f * m_spec.mass * (m_spec.frontGripFactor + m_DriveFactor[m_DriveMode, 1].frontGripFactor)
                                * (m_ctrl.getRealSteer() * m_spec.getMaxSteerRad() * num2 - num7 - num5);
                            num9 = 9.8f * m_spec.mass * (m_spec.rearGripFactor + m_DriveFactor[m_DriveMode, 1].rearGripFactor)
                                * (-num7 + num6);
                            num8 *= m_spec.driftSlipFactor * m_DriveFactor[m_DriveMode, 1].driftSlipFactor;
                            num9 *= m_spec.driftSlipFactor * m_DriveFactor[m_DriveMode, 1].driftSlipFactor;
                        }
                        else
                        {
                            num8 = 9.8f * m_spec.mass * m_spec.frontGripFactor * (m_ctrl.steerAngle * num2 - num7 - num5);
                            num9 = 9.8f * m_spec.mass * m_spec.rearGripFactor * (-num7 + num6);
                            num8 *= m_spec.driftSlipFactor;
                            num9 *= m_spec.driftSlipFactor;
                        }
                        z = (m_first.speed <= 10f)
                            ? (-(num8 + num9) * m_spec.driftLeanFactor * 0.5f)
                            : (-(num8 + num9) * m_spec.driftLeanFactor);
                        m_drift.slipTime = Mathf.Max(m_drift.slipTime - deltaT, 0f);
                    }
                    else
                    {
                        flag3 = true;
                        if (m_DriveMode > 0U)
                        {
                            float num17 = num / m_DriveFactor[m_DriveMode, 0].speedLimit;
                            float num18 = m_slip[0] * num17 * num17 + m_slip[1];
                            if (num18 > 1f)
                            {
                                num18 = 1f;
                            }
                            float num19;
                            if (m_DriveMode == 2U)
                            {
                                num19 = m_ctrl.getRealSteer() * m_spec.getMaxSteerRad() * num18;
                            }
                            else
                            {
                                num19 = m_ctrl.steerAngle * (1f + num18);
                            }
                            num8 = 9.8f * m_spec.mass * m_spec.frontGripFactor * (num19 * num2 - num7 - num5);
                            num9 = 9.8f * m_spec.mass * m_spec.rearGripFactor * (-num7 + num6);
                            num8 *= m_spec.driftSlipFactor * m_DriveFactor[m_DriveMode, 0].driftSlipFactor;
                            num9 *= m_spec.driftSlipFactor * m_DriveFactor[m_DriveMode, 0].driftSlipFactor;
                        }
                        else
                        {
                            num8 = 9.8f * m_spec.mass * m_spec.frontGripFactor * (m_ctrl.steerAngle * num2 - num7 - num5);
                            num9 = 9.8f * m_spec.mass * m_spec.rearGripFactor * (-num7 + num6);
                        }
                        z = -(num8 + num9) * m_spec.steerLeanFactor;
                    }
                    m_NetWForce += m_ort * new Vector3(num8 + num9, 0f, (!flag3) ? 0f : (-Mathf.Abs(num8 + num9) * m_spec.cornerDrawFactor));
                    m_NetLTorque += new Vector3(0f, num3 * num8 - num4 * num9, z);
                }
                else
                {
                    m_drift.slipMode = false;
                    m_drift.slipTime = 0f;
                    m_drift.trigger = false;
                    m_drift.triggerTime = 0f;
                    float num20 = m_KartLAVel.y * num3 / 5f;
                    float num21 = m_KartLAVel.y * num4 / 5f;
                    float num22 = m_first.leftVel / 5f;
                    float num23 = 9.8f * m_spec.mass * m_spec.frontGripFactor * (((num >= 0.5f) ? (m_ctrl.steerAngle * num2) : 0f) - num22 - num20);
                    float num24 = 9.8f * m_spec.mass * m_spec.rearGripFactor * (-num22 + num21);
                    m_NetWForce += m_ort * new Vector3(num23 + num24, 0f, 0f);
                    m_NetLTorque += new Vector3(0f, num3 * num23 - num4 * num24, 0f);
                }

                if (flag)
                {
                    m_DriveMode = driveMode;
                }
            }
        }


        private void applyWheels()
        {
            if (!m_Contact) return;

            int wheelContacts = 0;
            for (int i = 0; i < 4; i++)
            {
                if (m_sus.wheelContact[i])
                    wheelContacts++;
            }

            if (wheelContacts == 0 || wheelContacts >= 4)
                return;

            for (int i = 0; i < 4; i++)
            {
                if (m_sus.wheelContact[i])
                    continue;

                Vector3 position = transform.position +
                    m_first.left * m_spec.width * m_sus.wheelOff[i].x * 0.8f + m_first.front * m_spec.length * m_sus.wheelOff[i].y * 0.8f + m_first.up * m_sus.maxTravel;
                position.y += 1;

                var hitInfos = Physics.RaycastAll(position, new Vector3(0.0f, -1.0f, 0.0f), 2.0f);
                if (0 < hitInfos.Length && hitInfos.Any(hit => Mathf.Abs(hit.normal.y) >= 0.65f))
                    wheelContacts++;
            }

            if (wheelContacts == 4)
                m_KartWLVel += new Vector3(0.0f, 0.18f, 0.0f);
        }

        private void calcKartTractionForce(float deltaT)
        {
            if (!m_extern.slip)
            {
                Vector3 forward = Vector3.Cross(m_first.left, m_sus.contactN);
                if (m_ctrl.getRealAccel() != 0f)
                {
                    {
                        m_NetWForce += forward * m_ctrl.getRealAccel() * (m_drift.forceSlip ? m_spec.driftEscapeForce : m_spec.forwardAccel);
                    }

                    if (m_first.frontVel < 0f)
                    {
                        if (m_drift.slipMode || m_drift.forceSlip)
                        {
                            m_NetWForce += m_first.front * m_first.speed * m_spec.mass * 9.8f;
                        }
                        else
                        {
                            m_NetWForce += m_first.front * Mathf.Min(5f, m_first.speed) * m_spec.mass * 9.8f;
                        }
                    }

                    m_ctrl.stayTime = 0f;
                    return;
                }
                if (m_ctrl.getRealBrake() != 0f)
                {
                    bool flag = true;
                    if (m_first.frontVel < 0.5f)
                    {
                        m_ctrl.stayTime = m_ctrl.stayTime + deltaT;
                        if (m_first.frontVel < -0.5f)
                        {
                            m_ctrl.stayTime = 1f;
                        }
                        if (m_ctrl.stayTime > 0.2f)
                        {
                            {
                                m_NetWForce += forward * m_ctrl.getRealBrake() * -m_spec.backwardAccel;
                                flag = false;
                            }
                        }
                        else if (Mathf.Abs(m_first.leftVel) < 0.2f)
                        {
                            m_KartWLVel = Vector3.zero;
                            flag = false;
                        }
                    }
                    if (flag || m_extern.speedLimit > 0f && m_KartWLVel.sqrMagnitude > 0f)
                    {
                        Vector3 vector2 = m_KartWLVel.normalized;
                        vector2 -= Vector3.Dot(vector2, m_first.up) * m_first.up;
                        if (Vector3.Dot(vector2, forward) > 0.8f)
                        {
                            m_NetWForce -= vector2 * m_spec.gripBrake;
                            return;
                        }
                        m_NetWForce -= vector2 * m_spec.slipBrake;
                        return;
                    }
                }
                else if (m_first.frontVel <= 0.5f && m_first.frontVel >= -0.5f)
                {
                    m_ctrl.stayTime = m_ctrl.stayTime + deltaT;
                }
            }
        }

        private void calcNonpenetrateForce(float deltaT)
        {
            for (int i = 0; i < 4; i++)
            {
                float upScalar;
                if (m_sus.wheelContact[i])
                {
                    upScalar = m_sus.deltaTravel[i] > 0f
                        ? (m_spec.springK * m_sus.travel[i] + m_spec.damperCopC * (m_sus.deltaTravel[i] / deltaT))
                        : (m_spec.springK * m_sus.travel[i] + m_spec.damperRebC * (m_sus.deltaTravel[i] / deltaT));
                }
                else
                {
                    upScalar = 0f;
                }
                upScalar = upScalar > 0f
                    ? (Vector3.Dot(m_sus.wheelContactN[i], m_first.up) * upScalar)
                    : 0f;
                Vector3 b = Vector3.Cross(
                    new Vector3(m_spec.width * m_sus.wheelOff[i].x, 0f, -m_spec.length * m_sus.wheelOff[i].y),
                    new Vector3(0f, upScalar, 0f)) * 0.1f;
                m_NetLTorque += b;
            }
            m_NetWForce += m_theGravity * m_spec.mass * m_extern.gravityFactor * 0.8f;
        }

        private void applyForces(float deltaT)
        {
            // slipBoost
            transform.position += m_KartWLVel * deltaT;

            var rotation = transform.rotation;
            transform.rotation *= Quaternion.AngleAxis(m_KartLAVel.x * deltaT, m_first.left);

            {
                for (int i = 0; i < 4 && transform.up.y < 0.5f; i++)
                {
                    if (i == 3)
                    {
                        m_KartLAVel.x = 0.0f;
                        m_KartLAVel.z = 0.0f;
                    }
                    else
                    {
                        m_KartLAVel.x *= 0.1f;
                        m_KartLAVel.z *= 0.1f;
                    }

                    transform.rotation = rotation;
                    transform.rotation *= Quaternion.AngleAxis(m_KartLAVel.x * deltaT, m_first.left);
                }
            }

            /*
            Vector3 right = transform.right;
            Vector3 forward1 = transform.forward;
            Vector3 forward2 = Vector3.Cross( right, transform.up );
            transform.localRotation = Quaternion.Slerp( transform.localRotation, Quaternion.LookRotation( forward2, transform.up ), deltaT );

            {
                for( int i = 0; i < 4 && transform.up.y < 0.5f; i++ )
                {
                    if( i == 3 )
                    {
                        m_KartLAVel.x = 0.0f;
                        m_KartLAVel.z = 0.0f; 
                    }
                    else
                    {
                        m_KartLAVel.x *= 0.1f;
                        m_KartLAVel.z *= 0.1f;
                    }

                    right = transform.right;
                    forward1 = transform.forward;
                    forward2 = Vector3.Cross( right, transform.up );
                    transform.localRotation = Quaternion.Slerp( transform.localRotation, Quaternion.LookRotation( forward2, transform.up ), deltaT );
                }
            }
            */
        }

        private void endNetForce(float deltaT)
        {
            m_NetWForce += m_extern.annexForce;
            m_KartWLVel += m_NetWForce / m_spec.mass * deltaT;
            m_KartLAVel += Vector3.Scale(m_reciprocalMass, (m_NetLTorque - Vector3.Cross(m_KartLAVel, Vector3.Scale(m_reciprocalMass, m_KartLAVel)))) * deltaT;
        }

        private void calcFlyingKartForce()
        {
            m_drift.slipMode = false;
            m_drift.forceSlip = false;
            m_drift.trigger = false;

            m_NetWForce += m_theGravity * m_spec.mass * m_extern.gravityFactor;
            m_NetLTorque -= m_KartLAVel * 30.0f;
            m_ctrl.oldSteerAngle = 0.0f;
        }

        private bool decideKartContact(float deltaT)
        {
            m_cState.shock = m_Contact;
            m_Contact = false;
            for (uint num = 0U; num < 4U; num++)
            {
                Vector3 origin = transform.position + m_first.left * m_spec.width * m_sus.wheelOff[num].x * 0.8f + m_first.front * m_spec.length * m_sus.wheelOff[num].y * 0.8f + m_first.up * m_sus.maxTravel + m_first.up;
                float distance = m_sus.maxTravel * 2f + 1f;
                RaycastHit raycastHit;
                m_sus.wheelContact[num] = Physics.Raycast(origin, -m_first.up, out raycastHit, distance);
                if (m_sus.wheelContact[num])
                {
                    m_Contact = true;
                    m_sus.wheelContactN[num] = raycastHit.normal;
                    float a = Vector3.Dot(raycastHit.point, m_first.up) - (Vector3.Dot(transform.position, m_first.up) - m_sus.maxTravel);
                    float num2 = m_sus.travel[num];
                    m_sus.travel[num] = Mathf.Max(0f, Mathf.Min(a, m_sus.maxTravel * 2f));
                    m_sus.deltaTravel[num] = m_sus.travel[num] - num2;
                }
                else
                {
                    m_sus.travel[num] = 0f;
                    m_sus.deltaTravel[num] = 0f;
                }
                bool flag = m_sus.wheelContact[num];
            }
            m_sus.contactN = Vector3.zero;
            if (m_Contact)
            {
                uint num3 = 0U;
                for (uint num4 = 0U; num4 < 4U; num4 += 1U)
                {
                    if (m_sus.wheelContact[num4])
                    {
                        num3 += 1U;
                        m_sus.contactN = m_sus.contactN + m_sus.wheelContactN[num4];
                    }
                }
                m_sus.contactN = m_sus.contactN / num3;
                m_cState.shock = !m_cState.shock;
                if (num3 > 2U && m_cState.hop)
                {
                    m_cState.hop = false;
                }
            }
            else
            {
                m_cState.shock = false;
            }
            return m_Contact;
        }

        private void beginNetForce(float deltaT)
        {
            m_NetWForce = m_extern.force;
            m_NetLTorque = m_extern.torque;
            m_extern.force = Vector3.zero;
            m_extern.torque = Vector3.zero;
            m_extern.liftVel = Vector3.zero;
            m_first.left = transform.right;
            m_first.front = transform.forward;
            m_first.up = transform.up;
            m_first.frontVel = Vector3.Dot(m_KartWLVel, m_first.front);
            m_first.leftVel = Vector3.Dot(m_KartWLVel, m_first.left);
            m_first.upVel = Vector3.Dot(m_KartWLVel, m_first.up);
            m_first.speed = m_KartWLVel.magnitude;
            m_ort.setCol(m_first.left, m_first.up, m_first.front);
        }

        public void ResetPhysicsValues()
        {
            _lastCollisionPos = transform.position;
            _targetUpVector = Vector3.up;
            grounded = false;

            isBackupStreer = new bool[3];

            prevState_ = new PrevState();

            _steerLeft = false;
            _steerRight = false;
            _driftPressed = false;
            m_theGravity = new Vector3(0.0f, -49f, 0.0f);
            m_KartLAVel = Vector3.zero;
            m_KartWLVel = Vector3.zero;
            m_NetWForce = Vector3.zero;
            m_NetLTorque = Vector3.zero;
            m_boostLeft = 0;
            m_slipBoost = false;
            m_Contact = true;
            m_spec.Initialize();
            m_sus.Initialize();
            m_drift.Initialize();
            m_ctrl.Initialize();
            m_cState.Initialize();
            m_extern.Initialize();
            m_DriveFactor = new DriveFactor[3, 2];
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < 2; ++index2)
                    m_DriveFactor[index1, index2].Initialize();
            }
            m_stuckHelper.Initialize();
            m_ort = Matrix3.CreateMtxIdentity();

            m_slipReserveTime = 0.0f;
            m_steer = m_grip = 0.0f;
            m_slip[0] = 2f;
            m_slip[1] = 0.5f;
            m_DriveFactor[1, 0].speedLimit = 340f;
            m_DriveFactor[1, 0].betaCut = 0.6f;
            m_DriveFactor[1, 0].frontGripFactor = -1f;
            m_DriveFactor[1, 0].rearGripFactor = -1f;
            m_DriveFactor[1, 1].betaCut = 0.85f;
            m_DriveFactor[2, 0].speedLimit = 180f;
            m_DriveFactor[2, 0].betaCut = 0.6f;
            m_DriveFactor[2, 0].driftSlipFactor = 0.5f;
            m_DriveFactor[2, 1].frontGripFactor = -2f;
            m_DriveFactor[2, 1].rearGripFactor = -2f;

            m_spec.springK = (float)((double)m_spec.mass * (double)Mathf.Abs(m_theGravity.y) * 0.5);
            m_spec.damperCopC = 0.0f;
            m_spec.damperRebC = m_spec.springK * 0.2f;
            m_reciprocalMass = Vector3.zero;
            m_reciprocalMass.x = 12f / m_spec.mass;
            m_reciprocalMass.y = 12f / m_spec.mass;
            m_reciprocalMass.z = 12f / m_spec.mass;

            m_spec.width = 1.693220616f * 0.5f;
            m_spec.length = 2.52434444f * 0.5f;
        }

        private void ServerReconciliation()
        {
            // 서버 보정 로직: 클라이언트와 서버 간의 위치 및 상태 차이를 보정
            if (IsServer)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    var clientKart = client.PlayerObject.GetComponent<GoKartController>();
                    if (clientKart != null)
                    {
                        var serverKartPosition = clientKart.transform.position;
                        var serverKartRotation = clientKart.transform.rotation;

                        if (Vector3.Distance(clientKart.transform.position, serverKartPosition) > 0.1f ||
                            Quaternion.Angle(clientKart.transform.rotation, serverKartRotation) > 5f)
                        {
                            clientKart.transform.position = serverKartPosition;
                            clientKart.transform.rotation = serverKartRotation;
                        }
                    }
                }
            }
        }
    }
}