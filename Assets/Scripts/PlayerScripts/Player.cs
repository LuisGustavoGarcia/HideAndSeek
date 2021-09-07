
using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariableVector2 Position = new NetworkVariableVector2();

    public NetworkVariableBool IsSeeker = new NetworkVariableBool();

    public NetworkVariableBool IsCaught = new NetworkVariableBool();

    public NetworkVariableString Transformation = new NetworkVariableString();

    public float m_moveSpeed = 5f;

    [SerializeField] private GameObject m_seekerNameplate;
    [SerializeField] private GameObject m_minimumFieldOfViewSphere; 

    private Rigidbody2D m_rb;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private Vector2 m_currentMovement;
    private Vector2 m_previousMovement;
    private PlayerInputActions m_input;
    

    void Awake()
    {
        m_seekerNameplate.SetActive(false);
        m_rb = gameObject.GetComponent<Rigidbody2D>();
        m_animator = gameObject.GetComponent<Animator>();
        m_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (NetworkManager.Singleton.IsClient)
        {
            m_input = new PlayerInputActions();
        }
    }

    void OnEnable()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            m_input.Enable();
        }
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            m_input.Disable();
        }
    }

    public override void NetworkStart()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            FieldOfView.Singleton.gameObject.SetActive(true);
        }

        // Adjust field of view.
        if (GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            m_minimumFieldOfViewSphere.transform.localScale = new Vector3(100, 100, 1);
        }
        else
        {
            Destroy(m_minimumFieldOfViewSphere);
        }

        IsSeeker.OnValueChanged += UpdateSeekerStatus;
        Transformation.OnValueChanged += UpdateTransformation;
        MoveToStartingPosition();

        
    }

    public void MoveToStartingPosition()
    {
        // Server decides a valid spawn position.
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomValidPosition();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
    }

    [ServerRpc]
    void SubmitCurrentPositionServerRpc(Vector2 position, ServerRpcParams rpcParams = default)
    {
        Position.Value = position;
    }

    static Vector2 GetRandomValidPosition()
    {
        return new Vector2(Random.Range(-3f, 3f), 1f);
    }

    void Update()
    {
        if (IsLocalPlayer)
        {
            // Take user input to be used for movement.
            TakeUserInput();
            // Tell the server our client's position.
            SubmitCurrentPositionServerRpc(new Vector2(transform.position.x, transform.position.y));
            // Animate player character.
            UpdateAnimator();
            // Update Field of View.
            UpdateFieldOfView();
        }
    }

    void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            MovePlayer();
        }
    }

    void TakeUserInput()
    {
        if (m_currentMovement.x != 0 || m_currentMovement.y != 0)
        {
            m_previousMovement = m_currentMovement;
        }
        m_currentMovement = m_input.Desktop.Movement.ReadValue<Vector2>();
    }

    void MovePlayer()
    {
        m_rb.MovePosition(m_rb.position + m_currentMovement * m_moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateAnimator()
    {
        m_animator.SetFloat("CurrentHorizontal", m_currentMovement.x);
        m_animator.SetFloat("CurrentVertical", m_currentMovement.y);
        m_animator.SetFloat("PreviousHorizontal", m_previousMovement.x);
        m_animator.SetFloat("PreviousVertical", m_previousMovement.y);
    }

    void UpdateFieldOfView()
    {
        FieldOfView.Singleton.SetOrigin(transform.position);
        FieldOfView.Singleton.SetAimDirection(new Vector3(m_previousMovement.x, m_previousMovement.y, 0));
    }

    private void UpdateSeekerStatus(bool wasSeeker, bool isSeeker)
    {
        m_seekerNameplate.SetActive(isSeeker);
    }

    public void UpdateTransformation(string previousValue, string currentValue)
    {
        TransformationManager.Singleton.UpdatePlayerModel(Transformation.Value, m_animator, m_spriteRenderer);
    }
    
    [ClientRpc]
    public void ToggleFieldOfViewClientRpc(bool isEnabled, ClientRpcParams clientRpcParams = default)
    {
        if (isEnabled && GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            m_minimumFieldOfViewSphere.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            m_minimumFieldOfViewSphere.transform.localScale = new Vector3(100, 100, 1);
        }
    }
}