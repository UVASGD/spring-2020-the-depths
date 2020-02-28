using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController.v2_5D
{
    [vClassHeader("2.5D CONTROLLER")]
    public class v2_5DController : vThirdPersonController
    {
        [vEditorToolbar("Cursor Options")]
        public bool rotateToCursorInStrafe = true;
        [vHideInInspector("rotateToCursorInStrafe")]
        public LayerMask cursorLayerMask = 1 << 0;
     
        public v2_5DPath path;

        internal Vector3 targetForward;
        public override void Init()
        {
            base.Init();
            path = FindObjectOfType<v2_5DPath>();
            if (path) InitPath();
        }
       
        protected virtual void InitPath()
        {           
            transform.position = path.ConstraintPosition(transform.position);
            RotateToDirection(path.reference.right);
        }

        /// <summary>
        /// Main Camera
        /// </summary>
        public virtual Camera cameraMain
        {
            get
            {
                return vMousePositionHandler.Instance.mainCamera;
            }
        }

        /// <summary>
        /// Get cursor position on the screen
        /// </summary>
        public virtual Vector2 cursorPosition
        {
            get
            {
                return vMousePositionHandler.Instance.mousePosition;
            }
        }

        /// <summary>
        /// Get World position of the cursor relative to Controller transform
        /// </summary>
        public virtual Vector3 worldCursorPosition
        {
            get
            {
                return cursorRelativeToTransform;
            }
        }

        /// <summary>
        /// Get Local position of the cursor relative to Controller transform
        /// </summary>
        public virtual Vector3 localCursorPosition
        {
            get
            {
                var localPos = transform.InverseTransformPoint(cursorRelativeToTransform);

                return localPos;
            }
        }      

        protected virtual Vector3 cursorDirection
        {
           get
            {
                Vector3 selfLocal = cameraMain.transform.InverseTransformPoint(transform.position);
                Vector3 cursorLocal = cameraMain.transform.InverseTransformPoint(worldCursorPosition);
                if (cursorLocal.x > selfLocal.x + 0.1f) return targetForward= path.reference.right;
                else if (cursorLocal.x < selfLocal.x - 0.1f) return targetForward= - path.reference.right;
                else return targetForward;

            }
        }

        protected virtual Vector3 cursorRelativeToTransform
        {
            get
            {
                Vector3 cursorWordPosition = cameraMain.ScreenToWorldPoint(new Vector3(cursorPosition.x, cursorPosition.y,cameraMain.nearClipPlane));
                Vector3 camToTrans = transform.position - cameraMain.transform.position;
                Vector3 cursorDirFromCamera = cursorWordPosition - cameraMain.transform.position;
                return cameraMain.transform.position +cursorDirFromCamera * (Vector3.Dot(cameraMain.transform.forward, camToTrans) / Vector3.Dot(cameraMain.transform.forward, cursorDirFromCamera));
            }
        }

        public override void ControlLocomotionType()
        {
            base.ControlLocomotionType();
            if (!isDead && !ragdolled) transform.position = Vector3.Lerp(transform.position, path.ConstraintPosition(transform.position), 80 * Time.deltaTime);
        }        

        public override void ControlRotationType()
        {
            if (lockAnimRotation || customAction || isRolling) return;

            bool validInput = input != Vector3.zero || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

            if (validInput)
            {
                bool useStrafe = (isStrafing && rotateToCursorInStrafe && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero));
                Vector3 dir = useStrafe ? cursorDirection:path? path.reference.right*input.x:moveDirection;
                RotateToDirection(dir);
            }
        }

        public override void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (isRolling && !rollControl) return;
              
                var _forward =path?path.reference.right: referenceTransform?referenceTransform.right:Vector3.right;            
               moveDirection = (inputSmooth.x * _forward);
        }
    }
}

