// GENERATED AUTOMATICALLY FROM 'Assets/Input/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Movement"",
            ""id"": ""afa5a875-727e-4d32-b86a-5a99b8030c49"",
            ""actions"": [
                {
                    ""name"": ""Run"",
                    ""type"": ""PassThrough"",
                    ""id"": ""01cf73a3-19cf-4573-bb07-43f6f9ea5d57"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""185f3f25-0c97-47be-81b1-c23e362d83d9"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""fa544250-8cc0-4866-bf0c-4ef194ba1e46"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""8fcfcc3b-b8d3-41a8-9b89-2c93cdcaec66"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""b0dd2867-d47e-4354-8b75-bf787916274f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e9214722-cd31-49c1-b62e-d0dff9f6ce04"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""55f9c63b-2003-47b7-a1d9-c1e0a90f987b"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone"",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Skills"",
            ""id"": ""c48517fe-d95f-4c4f-a2f3-55a2d2fd899f"",
            ""actions"": [
                {
                    ""name"": ""CastQ"",
                    ""type"": ""Button"",
                    ""id"": ""1d55a63f-1cd3-49df-9aee-ee7b9ae3a41d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CastE"",
                    ""type"": ""Button"",
                    ""id"": ""45295559-2cd4-487f-addf-a4c49a296694"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InteractNPC"",
                    ""type"": ""Button"",
                    ""id"": ""edc31688-1754-4f3d-b477-f8042beb20da"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""51701f27-868d-4fef-a078-9d52e72ae3c5"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CastQ"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""064d5acf-aee3-48e8-8e30-28beb5c1ef2f"",
                    ""path"": ""<DualShockGamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CastQ"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""54c46a0e-32c7-4b1b-8a67-455a4268fdd3"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CastE"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cf6351bb-7c08-4899-8a75-d5a87f6363e3"",
                    ""path"": ""<DualShockGamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CastE"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9be406d6-317a-4189-b5ee-8b8672c12a98"",
                    ""path"": ""<DualShockGamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InteractNPC"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3da634e6-da6a-4679-a716-32814211833a"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InteractNPC"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UIs"",
            ""id"": ""54d88080-8bba-43d7-bb06-58618c4581e1"",
            ""actions"": [
                {
                    ""name"": ""ToggleInventory"",
                    ""type"": ""Button"",
                    ""id"": ""abfab3e1-68be-4d24-8d12-9f6e2ff57207"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4f109666-822f-4582-a410-8683105f9b18"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleInventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3d796970-b2f1-4447-8fd0-7e454faff6d6"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleInventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Movement
        m_Movement = asset.FindActionMap("Movement", throwIfNotFound: true);
        m_Movement_Run = m_Movement.FindAction("Run", throwIfNotFound: true);
        // Skills
        m_Skills = asset.FindActionMap("Skills", throwIfNotFound: true);
        m_Skills_CastQ = m_Skills.FindAction("CastQ", throwIfNotFound: true);
        m_Skills_CastE = m_Skills.FindAction("CastE", throwIfNotFound: true);
        m_Skills_InteractNPC = m_Skills.FindAction("InteractNPC", throwIfNotFound: true);
        // UIs
        m_UIs = asset.FindActionMap("UIs", throwIfNotFound: true);
        m_UIs_ToggleInventory = m_UIs.FindAction("ToggleInventory", throwIfNotFound: true);
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

    // Movement
    private readonly InputActionMap m_Movement;
    private IMovementActions m_MovementActionsCallbackInterface;
    private readonly InputAction m_Movement_Run;
    public struct MovementActions
    {
        private @PlayerControls m_Wrapper;
        public MovementActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Run => m_Wrapper.m_Movement_Run;
        public InputActionMap Get() { return m_Wrapper.m_Movement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MovementActions set) { return set.Get(); }
        public void SetCallbacks(IMovementActions instance)
        {
            if (m_Wrapper.m_MovementActionsCallbackInterface != null)
            {
                @Run.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnRun;
            }
            m_Wrapper.m_MovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
            }
        }
    }
    public MovementActions @Movement => new MovementActions(this);

    // Skills
    private readonly InputActionMap m_Skills;
    private ISkillsActions m_SkillsActionsCallbackInterface;
    private readonly InputAction m_Skills_CastQ;
    private readonly InputAction m_Skills_CastE;
    private readonly InputAction m_Skills_InteractNPC;
    public struct SkillsActions
    {
        private @PlayerControls m_Wrapper;
        public SkillsActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @CastQ => m_Wrapper.m_Skills_CastQ;
        public InputAction @CastE => m_Wrapper.m_Skills_CastE;
        public InputAction @InteractNPC => m_Wrapper.m_Skills_InteractNPC;
        public InputActionMap Get() { return m_Wrapper.m_Skills; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SkillsActions set) { return set.Get(); }
        public void SetCallbacks(ISkillsActions instance)
        {
            if (m_Wrapper.m_SkillsActionsCallbackInterface != null)
            {
                @CastQ.started -= m_Wrapper.m_SkillsActionsCallbackInterface.OnCastQ;
                @CastQ.performed -= m_Wrapper.m_SkillsActionsCallbackInterface.OnCastQ;
                @CastQ.canceled -= m_Wrapper.m_SkillsActionsCallbackInterface.OnCastQ;
                @CastE.started -= m_Wrapper.m_SkillsActionsCallbackInterface.OnCastE;
                @CastE.performed -= m_Wrapper.m_SkillsActionsCallbackInterface.OnCastE;
                @CastE.canceled -= m_Wrapper.m_SkillsActionsCallbackInterface.OnCastE;
                @InteractNPC.started -= m_Wrapper.m_SkillsActionsCallbackInterface.OnInteractNPC;
                @InteractNPC.performed -= m_Wrapper.m_SkillsActionsCallbackInterface.OnInteractNPC;
                @InteractNPC.canceled -= m_Wrapper.m_SkillsActionsCallbackInterface.OnInteractNPC;
            }
            m_Wrapper.m_SkillsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @CastQ.started += instance.OnCastQ;
                @CastQ.performed += instance.OnCastQ;
                @CastQ.canceled += instance.OnCastQ;
                @CastE.started += instance.OnCastE;
                @CastE.performed += instance.OnCastE;
                @CastE.canceled += instance.OnCastE;
                @InteractNPC.started += instance.OnInteractNPC;
                @InteractNPC.performed += instance.OnInteractNPC;
                @InteractNPC.canceled += instance.OnInteractNPC;
            }
        }
    }
    public SkillsActions @Skills => new SkillsActions(this);

    // UIs
    private readonly InputActionMap m_UIs;
    private IUIsActions m_UIsActionsCallbackInterface;
    private readonly InputAction m_UIs_ToggleInventory;
    public struct UIsActions
    {
        private @PlayerControls m_Wrapper;
        public UIsActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ToggleInventory => m_Wrapper.m_UIs_ToggleInventory;
        public InputActionMap Get() { return m_Wrapper.m_UIs; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIsActions set) { return set.Get(); }
        public void SetCallbacks(IUIsActions instance)
        {
            if (m_Wrapper.m_UIsActionsCallbackInterface != null)
            {
                @ToggleInventory.started -= m_Wrapper.m_UIsActionsCallbackInterface.OnToggleInventory;
                @ToggleInventory.performed -= m_Wrapper.m_UIsActionsCallbackInterface.OnToggleInventory;
                @ToggleInventory.canceled -= m_Wrapper.m_UIsActionsCallbackInterface.OnToggleInventory;
            }
            m_Wrapper.m_UIsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ToggleInventory.started += instance.OnToggleInventory;
                @ToggleInventory.performed += instance.OnToggleInventory;
                @ToggleInventory.canceled += instance.OnToggleInventory;
            }
        }
    }
    public UIsActions @UIs => new UIsActions(this);
    public interface IMovementActions
    {
        void OnRun(InputAction.CallbackContext context);
    }
    public interface ISkillsActions
    {
        void OnCastQ(InputAction.CallbackContext context);
        void OnCastE(InputAction.CallbackContext context);
        void OnInteractNPC(InputAction.CallbackContext context);
    }
    public interface IUIsActions
    {
        void OnToggleInventory(InputAction.CallbackContext context);
    }
}
