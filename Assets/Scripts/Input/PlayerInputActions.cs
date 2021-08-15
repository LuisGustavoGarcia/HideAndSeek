// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Desktop"",
            ""id"": ""0ace6e52-9b77-4e0c-8e41-e06e8e61dea5"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""46d4935e-72e1-4b68-a3e5-22c59aded70f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""0ff7753a-f3b4-4a8a-b97a-23fa90ee9cce"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""cee64267-d673-49d7-a1d3-9c6aabcd4b28"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""cb49f10e-02d7-460e-8243-2178b2cab1eb"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""28ce7533-8264-4c25-8cc5-4d358857fc03"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""b389a66d-38ee-4e70-831b-de19dbb22c3c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""085d8e9f-9354-45e5-a901-f3b8a73335a1"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""7f839a41-c708-4987-8610-1ed18d348b7d"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Desktop
        m_Desktop = asset.FindActionMap("Desktop", throwIfNotFound: true);
        m_Desktop_Movement = m_Desktop.FindAction("Movement", throwIfNotFound: true);
        m_Desktop_Click = m_Desktop.FindAction("Click", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Desktop
    private readonly InputActionMap m_Desktop;
    private IDesktopActions m_DesktopActionsCallbackInterface;
    private readonly InputAction m_Desktop_Movement;
    private readonly InputAction m_Desktop_Click;
    public struct DesktopActions
    {
        private @PlayerInputActions m_Wrapper;
        public DesktopActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_Desktop_Movement;
        public InputAction @Click => m_Wrapper.m_Desktop_Click;
        public InputActionMap Get() { return m_Wrapper.m_Desktop; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DesktopActions set) { return set.Get(); }
        public void SetCallbacks(IDesktopActions instance)
        {
            if (m_Wrapper.m_DesktopActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnMovement;
                @Click.started -= m_Wrapper.m_DesktopActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_DesktopActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_DesktopActionsCallbackInterface.OnClick;
            }
            m_Wrapper.m_DesktopActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
            }
        }
    }
    public DesktopActions @Desktop => new DesktopActions(this);
    public interface IDesktopActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
    }
}
