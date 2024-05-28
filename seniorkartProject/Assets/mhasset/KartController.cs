using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KartController : MonoBehaviour
{
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

        public float getMaxSteerRad() => (float)( 3.1415927410125732 * (double)maxSteerDeg / 180.0 );
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

        public void InitVector2( ref Vector2 v, float x, float y )
        {
            v.x = x;
            v.y = y;
        }

        public void Initialize()
        {
            wheelOff = new Vector2[ 4 ];
            for( int index = 0; index < 4; ++index )
                InitVector2( ref wheelOff[ index ], index % 2 != 0 ? 1f : -1f, index / 2 != 0 ? -1f : 1f );

            wheelContact = new bool[ 4 ];
            wheelContactN = new Vector3[ 4 ];
            maxTravel = 0.5f;
            travel = new float[ 4 ] { 0.5f, 0.5f, 0.5f, 0.5f };
            deltaTravel = new float[ 4 ];
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

        public float getRealSteer() => ( wheelFlip || wheelDevil ? -1f : 1f ) * steer;

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
    private float[] m_slip = new float[ 2 ];
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

    // Start is called before the first frame update
    void Start()
    {
        ResetPhysicsValues();
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetKeyDown( KeyCode.UpArrow ) )
        {
            setAccel( true );
        }
        else if( Input.GetKeyUp( KeyCode.UpArrow ) )
        {
            setAccel( false );
        }

        if( Input.GetKeyDown( KeyCode.DownArrow ) )
        {
            setBrake( true );
        }
        else if( Input.GetKeyUp( KeyCode.DownArrow ) )
        {
            setBrake( false );
        }

        if( Input.GetKeyDown( KeyCode.LeftArrow ) )
        {
            setSteer( -1.0f );
            _steerLeft = true;
            updateDriftPressed( true );
        }
        else if( Input.GetKeyUp( KeyCode.LeftArrow ) )
        {
            _steerLeft = false;
            setSteer( _steerLeft || _steerRight ? _steerLeft ? -1.0f : 1.0f : 0.0f );
            updateDriftPressed( false );
        }

        if( Input.GetKeyDown( KeyCode.RightArrow ) )
        {
            setSteer( 1.0f );
            _steerRight = true;
            updateDriftPressed( true );
        }
        else if( Input.GetKeyUp( KeyCode.RightArrow ) )
        {
            _steerRight = false;
            setSteer( _steerLeft || _steerRight ? _steerLeft ? -1.0f : 1.0f : 0.0f );
            updateDriftPressed( false );
        }

        if( Input.GetKeyDown( KeyCode.LeftShift ) )
        {
            _driftPressed = true;
            updateDriftPressed( true );
        }
        else if( Input.GetKeyUp( KeyCode.LeftShift ) )
        {
            _driftPressed = false;
            updateDriftPressed( false );
        }
    }

    private void updateDriftPressed( bool pressed )
    {
        if( !pressed )
        {
            setDrift( false );
        }
        else if( _driftPressed && m_ctrl.steer != 0 )
        {
            setDrift( true );
        }
    }


    public void setAccel( bool accel )
    {
        Debug.Log( "setAccel accel=" + accel );

        if( accel )
        {
            m_ctrl.accel = 1.0f;
            m_slipBoost = true;
        }
        else
        {
            m_ctrl.accel = 0.0f;
        }
    }

    public void setBrake( bool brake )
    {
        Debug.Log( "setBrake brake=" + brake );

        if( brake )
        {
            m_ctrl.brake = 1.0f;
        }
        else
        {
            m_ctrl.brake = 0.0f;
        }
    }

    public void setDrift( bool drift )
    {
        Debug.Log( "setDrift drift=" + drift );

        if( drift )
        {
            if( m_drift.slipTime <= 0.0f )
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

    public void setSteer( float value )
    {
        Debug.Log( "setSteer steer=" + value );

        m_ctrl.steer = value;
    }

    public void DrawBox( Vector3 pos, Quaternion rot, Vector3 scale, Color c )
    {
        // create matrix
        Matrix4x4 m = new Matrix4x4();
        m.SetTRS( pos, rot, scale );

        var point1 = m.MultiplyPoint( new Vector3( -0.5f, -0.5f, 0.5f ) );
        var point2 = m.MultiplyPoint( new Vector3( 0.5f, -0.5f, 0.5f ) );
        var point3 = m.MultiplyPoint( new Vector3( 0.5f, -0.5f, -0.5f ) );
        var point4 = m.MultiplyPoint( new Vector3( -0.5f, -0.5f, -0.5f ) );

        var point5 = m.MultiplyPoint( new Vector3( -0.5f, 0.5f, 0.5f ) );
        var point6 = m.MultiplyPoint( new Vector3( 0.5f, 0.5f, 0.5f ) );
        var point7 = m.MultiplyPoint( new Vector3( 0.5f, 0.5f, -0.5f ) );
        var point8 = m.MultiplyPoint( new Vector3( -0.5f, 0.5f, -0.5f ) );

        Debug.DrawLine( point1, point2, c );
        Debug.DrawLine( point2, point3, c );
        Debug.DrawLine( point3, point4, c );
        Debug.DrawLine( point4, point1, c );

        Debug.DrawLine( point5, point6, c );
        Debug.DrawLine( point6, point7, c );
        Debug.DrawLine( point7, point8, c );
        Debug.DrawLine( point8, point5, c );

        Debug.DrawLine( point1, point5, c );
        Debug.DrawLine( point2, point6, c );
        Debug.DrawLine( point3, point7, c );
        Debug.DrawLine( point4, point8, c );
    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        float deltaInMs = deltaTime * 1000.0f;
        float deltaRemain = deltaInMs;

        isCrash_ = false;
        isShock_ = false;
        crashVelocity_ = 0.0f;
        shockVelocity_ = 0.0f;
        m_isDrift = false;

        while( 0.0f < deltaRemain )
        {
            float stepDeltaMs = Mathf.Min( deltaRemain, 2.0f );
            deltaRemain -= stepDeltaMs;

            float deltaT = stepDeltaMs * 0.001f;

            _lastPhysicsFrame = deltaRemain <= 0.0f;

            // TODO boostLeft
            // processAdBoostTime

            beginNetForce( deltaT );

            if( decideKartContact( deltaT ) )
            {
                // Debug.Log( "!!!!contact" );
                calcNonpenetrateForce( deltaT );
                calcKartTractionForce( deltaT );
                calcKartSteeringForce( deltaT );
            }
            else
            {
                // Debug.Log( "!!!!fly" );
                calcFlyingKartForce();
            }

            calcResistForce();
            endNetForce( deltaT );

            m_KartWLVel += m_extern.liftVel * deltaT;

            bool collided = applyCollision( deltaT, 0.65f );

            applyWheels();
            applyForces( deltaT );
            m_KartWLVel -= m_extern.liftVel * deltaT;
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        Debug.Log( "OnTriggerEnter" );
    }

    private Vector3 _lastCollisionPos;
    private bool applyCollision( float deltaT, float wallZ )
    {
        bool result = false;
        Vector3 size = new Vector3( m_spec.width, 0.7f, m_spec.length );
        var rot = transform.localRotation;
        Vector3 top = transform.position + m_first.up;

        DrawBox( top, rot, size, Color.red );

        var deltaPos = transform.position - _lastCollisionPos;
        _lastCollisionPos = transform.position;


        var hits = Physics.BoxCastAll( top, size, deltaPos.normalized, rot, deltaPos.magnitude );
        foreach( var hit in hits )
        {
            // hack fix
            if( hit.collider.name.StartsWith( "CheckPoint" ) )
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
                float dot = Vector3.Dot( m_KartWLVel, normal );
                if( dot < 0.0f )
                {
                    m_ctrl.oldSteerAngle = 0.0f;
                    result = true;

                    Vector3 shock = normal * Vector3.Dot( m_KartWLVel, normal );
                    Vector3 shockOffset = m_KartWLVel - shock;

                    if( normal.y > wallZ )
                    {
                        m_KartWLVel = shock * -( Mathf.Abs( Vector3.Dot( m_first.front, normal ) ) * 0.7f ) + shockOffset;
                        m_cState.shockVel = shock.magnitude;
                        m_KartLAVel.x -= Vector3.Dot( Vector3.Cross( normal, m_first.up ), m_first.left ) * 0.1f;
                        m_KartLAVel.z -= Vector3.Dot( Vector3.Cross( normal, m_first.up ), -m_first.front ) * 0.1f;
                    }
                    else
                    {
                        float shockDist = shockOffset.magnitude;
                        Vector3 addSub = shockDist > 0 ? shockOffset.normalized : Vector3.zero;
                        Vector3 add = shock * -1.5f - addSub * Math.Min( shock.magnitude * 1.5f, shockDist * 0.6f );
                        add -= new Vector3( 0, 1, 0 ) * add.y;
                        if( m_KartWLVel.sqrMagnitude < 100 && !m_drift.slipMode && m_Contact )
                        {
                            bool crash = false;
                            float crashDir = 0;
                            float crashDir2 = 0;
                            if( m_ctrl.getRealAccel() == 1 )
                            {
                                if( m_ctrl.steer == 1 )
                                {
                                    crash = true;
                                    crashDir = m_ctrl.wheelFlip ? -1.0f : 1.0f;
                                    crashDir2 = 1;
                                }
                                else if( m_ctrl.steer == -1 )
                                {
                                    crash = true;
                                    crashDir = m_ctrl.wheelFlip ? 1.0f : -1.0f;
                                    crashDir2 = -1;
                                }
                            }
                            if( crash )
                            {
                                Vector3 up = normal;
                                Vector3 back = -m_first.front;
                                back.Normalize();
                                float crashMin = Vector3.Dot( up, back );
                                float crashVel = 0.0f;
                                Vector3 left = m_first.left * crashDir2;
                                if( Vector3.Dot( up, left ) < 0 )
                                {
                                    crashMin = 2 - crashMin;
                                    crashVel = Mathf.Max( crashMin, 1.5f );
                                }
                                else
                                {
                                    crashVel = crashMin * crashMin * crashMin;
                                }
                                add -= shock.normalized * ( 3 * crashMin + 1 );
                                add += m_first.left * ( 3 * crashMin + 1 ) * crashDir;
                                m_KartLAVel.y += ( 6 * crashVel + 2 ) * crashDir;
                                // m_driftOrCrash2 = true; // m_sound.driftOrCrash2 = true;
                                // m_crash2Vel = max( crashMin, 1.0f ) * 0.3f + 0.1f;
                            }
                        }
                        m_KartWLVel += add;

                        float dotFront = Vector3.Dot( normal, m_first.front );
                        float dotLeft = Vector3.Dot( normal, m_first.left );
                        if( Mathf.Abs( dotLeft ) < Mathf.Abs( dotFront * 0.8f ) )
                        {
                            add = Vector3.zero;
                            float dir = dotFront > 0 ? 1.0f : -1.0f;
                            float addValue = Mathf.Abs( Vector3.Dot( normal, m_KartWLVel ) * 0.5f );
                            add.y = dotLeft * dir * Mathf.Clamp( addValue, 1.0f, 30.0f );
                            if( Vector3.Dot( add, m_KartLAVel ) <= 1 )
                                m_KartLAVel += add;
                        }
                        else
                        {
                            add = Vector3.zero;
                            float dir = dotLeft > 0 ? -1.0f : 1.0f;
                            float addValue = Mathf.Abs( Vector3.Dot( normal, m_KartWLVel ) * 0.5f );
                            add.y = dotFront * dir * Mathf.Clamp( addValue, 1.0f, 30.0f );
                            if( Vector3.Dot( add, m_KartLAVel ) <= 1 )
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

        if( m_Contact )
            force -= m_KartWLVel
                * m_KartWLVel.magnitude
                * m_spec.dragFactor
                * m_extern.dragFactor
                * m_extern.compensationDragFactor;

        m_NetWForce += force;
        m_NetLTorque += torque;
    }

    private void calcKartSteeringForce( float deltaT )
    {

    }

    private void applyWheels()
    {
        if( !m_Contact ) return;

        int wheelContacts = 0;
        for( int i = 0; i < 4; i++ )
        {
            if( m_sus.wheelContact[ i ] )
                wheelContacts++;
        }

        if( wheelContacts == 0 || wheelContacts >= 4 )
            return;

        for( int i = 0; i < 4; i++ )
        {
            if( m_sus.wheelContact[ i ] )
                continue;

            Vector3 position = transform.position +
                m_first.left * m_spec.width * m_sus.wheelOff[ i ].x * 0.8f + m_first.front * m_spec.length * m_sus.wheelOff[ i ].y * 0.8f + m_first.up * m_sus.maxTravel;
            position.y += 1;

            var hitInfos = Physics.RaycastAll( position, new Vector3( 0.0f, -1.0f, 0.0f ), 2.0f );
            if( 0 < hitInfos.Length && hitInfos.Any( hit => Mathf.Abs( hit.normal.y ) >= 0.65f ) )
                wheelContacts++;
        }

        if( wheelContacts == 4 )
            m_KartWLVel += new Vector3( 0.0f, 0.18f, 0.0f );
    }

    private void calcKartTractionForce( float deltaT )
    {
        if( m_extern.slip )
            return;

        Vector3 forward = Vector3.Cross( m_first.left, m_sus.contactN );
        if( m_ctrl.getRealAccel() != 0f )
        {
            m_NetWForce += forward * m_ctrl.getRealAccel() * ( m_drift.forceSlip ? m_spec.driftEscapeForce : m_spec.forwardAccel );

            if( m_first.frontVel < 0f )
            {
                if( m_drift.slipMode || m_drift.forceSlip )
                {
                    m_NetWForce += m_first.front * m_first.speed * m_spec.mass * 9.8f;
                }
                else
                {
                    m_NetWForce += m_first.front * Mathf.Min( 5f, m_first.speed ) * m_spec.mass * 9.8f;
                }
            }

            m_ctrl.stayTime = 0f;
        }
        else if( m_ctrl.getRealBrake() != 0f )
        {
            bool flag = true;
            if( m_first.frontVel < 0.5f )
            {
                m_ctrl.stayTime = m_ctrl.stayTime + deltaT;
                if( m_first.frontVel < -0.5f )
                {
                    m_ctrl.stayTime = 1f;
                }
                if( m_ctrl.stayTime > 0.2f )
                {
                    m_NetWForce += forward * m_ctrl.getRealBrake() * -m_spec.backwardAccel;
                    flag = false;
                }
                else if( Mathf.Abs( m_first.leftVel ) < 0.2f )
                {
                    m_KartWLVel = Vector3.zero;
                    flag = false;
                }
            }
            if( flag || m_extern.speedLimit > 0f && m_KartWLVel.sqrMagnitude > 0f )
            {
                Vector3 vector2 = m_KartWLVel.normalized;
                vector2 -= Vector3.Dot( vector2, m_first.up ) * m_first.up;
                if( Vector3.Dot( vector2, forward ) > 0.8f )
                {
                    m_NetWForce -= vector2 * m_spec.gripBrake;
                }
                else
                {
                    m_NetWForce -= vector2 * m_spec.slipBrake;
                }
            }
        }
        else if( m_first.frontVel <= 0.5f && m_first.frontVel >= -0.5f )
        {
            m_ctrl.stayTime += deltaT;
        }
    }

    private void calcNonpenetrateForce( float deltaT )
    {
        for( int i = 0; i < 4; i++ )
        {
            float upScalar;
            if( m_sus.wheelContact[ i ] )
            {
                upScalar = m_sus.deltaTravel[ i ] > 0f
                    ? ( m_spec.springK * m_sus.travel[ i ] + m_spec.damperCopC * ( m_sus.deltaTravel[ i ] / deltaT ) )
                    : ( m_spec.springK * m_sus.travel[ i ] + m_spec.damperRebC * ( m_sus.deltaTravel[ i ] / deltaT ) );
            }
            else
            {
                upScalar = 0f;
            }
            upScalar = upScalar > 0f
                ? ( Vector3.Dot( m_sus.wheelContactN[ i ], m_first.up ) * upScalar )
                : 0f;

            m_NetWForce += m_first.up * upScalar;

            Vector3 b = Vector3.Cross(
                new Vector3( -m_spec.width * m_sus.wheelOff[ i ].x, 0f, m_spec.length * m_sus.wheelOff[ i ].y ),
                new Vector3( 0f, upScalar, 0f ) ) * 0.1f;
            m_NetLTorque += b;
        }
        m_NetWForce += m_theGravity * m_spec.mass * m_extern.gravityFactor * 0.8f;
    }

    private void applyForces( float deltaT )
    {
        // slipBoost
        transform.position += m_KartWLVel * deltaT;

        var rotation = transform.rotation;
        transform.rotation *= Quaternion.AngleAxis( m_KartLAVel.x * deltaT, m_first.left );

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

                transform.rotation = rotation;
                transform.rotation *= Quaternion.AngleAxis( m_KartLAVel.x * deltaT, m_first.left );
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

    private void endNetForce( float deltaT )
    {
        m_NetWForce += m_extern.annexForce;
        m_KartWLVel += m_NetWForce / m_spec.mass * deltaT;
        m_KartLAVel += Vector3.Scale( m_reciprocalMass, ( m_NetLTorque - Vector3.Cross( m_KartLAVel, Vector3.Scale( m_reciprocalMass, m_KartLAVel ) ) ) ) * deltaT;
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

    private bool decideKartContact( float deltaT )
    {
        m_cState.shock = m_Contact;
        m_Contact = false;

        for( int i = 0; i < 4; i++ )
        {
            Vector3 origin = transform.position +
                m_first.left * m_spec.width * m_sus.wheelOff[ i ].x * 0.8f + m_first.front * m_spec.length * m_sus.wheelOff[ i ].y * 0.8f + m_first.up * m_sus.maxTravel;
            float distance = m_sus.maxTravel * 2.0f;

            var hitInfos = Physics.RaycastAll( origin, -m_first.up, distance ).Where( hit => Mathf.Abs( hit.normal.y ) >= 0.65f ).OrderBy( hit => hit.distance );
            var hitInfo = hitInfos.FirstOrDefault();

            m_sus.wheelContact[ i ] = hitInfos.Any();
            Debug.DrawLine( origin, origin - m_first.up * distance, m_sus.wheelContact[ i ] ? Color.green : Color.red );
            if( m_sus.wheelContact[ i ] )
            {
                Debug.DrawLine( hitInfo.point, hitInfo.point + hitInfo.normal * 0.1f, Color.yellow );
                Debug.Log( "Wheel " + i + " Collided with " + hitInfo.collider?.gameObject?.name );

                m_Contact = true;
                m_sus.wheelContactN[ i ] = hitInfo.normal;

                float a = Vector3.Dot( hitInfo.point, m_first.up ) - ( Vector3.Dot( transform.position, m_first.up ) - m_sus.maxTravel );
                float prevTravel = m_sus.travel[ i ];
                m_sus.travel[ i ] = Mathf.Max( 0.0f, Mathf.Min( a, m_sus.maxTravel * 2f ) );
                m_sus.deltaTravel[ i ] = m_sus.travel[ i ] - prevTravel;
            }
            else
            {
                m_sus.travel[ i ] = m_sus.deltaTravel[ i ] = 0.0f;
                // Debug.Log( "Wheel " + i + " !! no collide " );
            }

            // m_sus.travel[ i ] = 0.45f;
            // m_sus.deltaTravel[ i ] = 0.0f;
        }

        Debug.Log( "travel=" + m_sus.travel[ 0 ] + "," + m_sus.travel[ 1 ] + "," + m_sus.travel[ 2 ] + "," + m_sus.travel[ 3 ] );
        Debug.Log( "deltaTravel=" + m_sus.deltaTravel[ 0 ] + "," + m_sus.deltaTravel[ 1 ] + "," + m_sus.deltaTravel[ 2 ] + "," + m_sus.deltaTravel[ 3 ] );

        m_sus.contactN = Vector3.zero;
        if( m_Contact )
        {
            int contactCount = 0;
            for( int i = 0; i < 4; i++ )
            {
                if( m_sus.wheelContact[ i ] )
                {
                    m_sus.contactN += m_sus.wheelContactN[ i ];
                    contactCount++;
                }
            }
            m_sus.contactN /= contactCount;
            m_cState.shock = !m_cState.shock;
        }
        else
        {
            m_cState.shock = false;
        }

        return m_Contact;
    }

    private void beginNetForce( float deltaT )
    {
        m_NetWForce = m_extern.force;
        m_NetLTorque = m_extern.torque;
        m_extern.force = Vector3.zero;
        m_extern.torque = Vector3.zero;
        m_extern.liftVel = Vector3.zero;
        m_first.left = transform.right;
        m_first.front = transform.forward;
        m_first.up = transform.up;
        m_first.frontVel = Vector3.Dot( m_KartWLVel, m_first.front );
        m_first.leftVel = Vector3.Dot( m_KartWLVel, m_first.left );
        m_first.upVel = Vector3.Dot( m_KartWLVel, m_first.up );
        m_first.speed = m_KartWLVel.magnitude;
        m_ort.setCol( m_first.left, m_first.up, m_first.front );
    }

    public void ResetPhysicsValues()
    {
        _lastCollisionPos = transform.position;

        _steerLeft = false;
        _steerRight = false;
        _driftPressed = false;
        m_theGravity = new Vector3( 0.0f, -49f, 0.0f );
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
        m_DriveFactor = new DriveFactor[ 3, 2 ];
        for( int index1 = 0; index1 < 3; ++index1 )
        {
            for( int index2 = 0; index2 < 2; ++index2 )
                m_DriveFactor[ index1, index2 ].Initialize();
        }
        m_stuckHelper.Initialize();
        m_ort = Matrix3.CreateMtxIdentity();

        m_slipReserveTime = 0.0f;
        m_steer = m_grip = 0.0f;
        m_slip[ 0 ] = 2f;
        m_slip[ 1 ] = 0.5f;
        m_DriveFactor[ 1, 0 ].speedLimit = 340f;
        m_DriveFactor[ 1, 0 ].betaCut = 0.6f;
        m_DriveFactor[ 1, 0 ].frontGripFactor = -1f;
        m_DriveFactor[ 1, 0 ].rearGripFactor = -1f;
        m_DriveFactor[ 1, 1 ].betaCut = 0.85f;
        m_DriveFactor[ 2, 0 ].speedLimit = 180f;
        m_DriveFactor[ 2, 0 ].betaCut = 0.6f;
        m_DriveFactor[ 2, 0 ].driftSlipFactor = 0.5f;
        m_DriveFactor[ 2, 1 ].frontGripFactor = -2f;
        m_DriveFactor[ 2, 1 ].rearGripFactor = -2f;

        m_spec.springK = (float)( (double)m_spec.mass * (double)Mathf.Abs( m_theGravity.y ) * 0.5 );
        m_spec.damperCopC = 0.0f;
        m_spec.damperRebC = m_spec.springK * 0.2f;
        m_reciprocalMass = Vector3.zero;
        m_reciprocalMass.x = 12f / m_spec.mass;
        m_reciprocalMass.y = 12f / m_spec.mass;
        m_reciprocalMass.z = 12f / m_spec.mass;

        m_spec.width = 1.693220616f * 0.5f;
        m_spec.length = 2.52434444f * 0.5f;
    }
}
