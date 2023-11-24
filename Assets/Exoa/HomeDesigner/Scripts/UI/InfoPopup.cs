using Exoa.Designer.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Exoa.Designer
{
    public class InfoPopup : BaseFloatingPopup
    {
        public static InfoPopup Instance;

        public static UnityEvent OnClickMove = new UnityEvent();
        public static UnityEvent OnClickDelete = new UnityEvent();
        public static UnityEvent OnClickColor = new UnityEvent();
        public static UnityEvent onClickMirror = new UnityEvent();
        public static UnityEvent onClickRotate = new UnityEvent();

        private RawImage thumb;
        private TMP_Text text;
        public Button moveBtn;
        public Button trashBtn;
        public Button variantsBtn;
        public Button colorButton;
        public Button mirrorButton;
        public Button rotateButton;

        override protected void Awake()
        {

            Instance = this;

            thumb = contentGo.GetComponentInChildren<RawImage>();
            text = contentGo.GetComponentInChildren<TMP_Text>();

            trashBtn?.onClick.AddListener(() =>
            {
                OnClickDelete.Invoke();
                Hide();
            });
            moveBtn?.onClick.AddListener(() =>
            {
                OnClickMove.Invoke();
                Hide();
            });
            variantsBtn?.onClick.AddListener(() =>
            {
                MaterialPopup.Instance.ShowMode(MaterialPopupUI.Mode.Module, CurrentTarget.gameObject);
                Hide();
            });
            colorButton?.onClick.AddListener(() =>
            {
                OnClickColor.Invoke();
                Hide();
            });
            mirrorButton?.onClick.AddListener(() =>
            {
                onClickMirror.Invoke();
            });
            rotateButton?.onClick.AddListener(() =>
            {
                onClickRotate.Invoke();
                Hide();
            });
            base.Awake();
        }


        public void Show(Transform target, ModuleDataModels.Module p, bool showButtons = false, bool moveWithMouse = true)
        {
            //print("info popup Show");
            CurrentTarget = target;
            this.moveWithMouse = moveWithMouse;
            text.text = "<b>" + p.title + "</b>\n";
            if (!string.IsNullOrEmpty(p.sku))
                text.text += "<i>SKU:" + p.sku + "</i>\n";
            text.text += p.description;
            thumb.texture = Resources.Load<Texture>(HDSettings.MODULE_THUMBNAIL_FOLDER + p.prefab);

            trashBtn?.gameObject.SetActive(showButtons);
            moveBtn?.gameObject.SetActive(showButtons);
            variantsBtn?.gameObject.SetActive(showButtons);
            mirrorButton?.gameObject.SetActive(showButtons);
            colorButton?.gameObject.SetActive(showButtons);
            rotateButton?.gameObject.SetActive(showButtons);

            // Disable the variant button if no variant are setup on the module
            if (target != null && showButtons)
            {
                ModuleColorVariants variantCp = target.gameObject.GetComponentInChildren<ModuleColorVariants>();
                if (variantCp == null || variantCp.variants == null || variantCp.variants.Count == 0)
                {
                    variantsBtn?.gameObject.SetActive(false);
                }
                else
                {
                    variantsBtn?.gameObject.SetActive(true);
                }
                if(target.transform.tag == "Wall" || target.transform.tag == "Floor")
                {
                    mirrorButton?.gameObject.SetActive(false);
                }
                else
                {
                    mirrorButton?.gameObject.SetActive(true);
                }
                _ColorController colorController = target.gameObject.GetComponent<_ColorController>();
                if(colorController == null)
                {
                    colorButton?.gameObject.SetActive(false);
                }
                else
                {
                    colorButton.gameObject.SetActive(true);
                }
                if (!target.GetComponent<ModuleController>().isMirror)
                {
                    mirrorButton?.gameObject.SetActive(false);
                }
            }

            rect.SetHeight(showButtons ? 188 : 140);
            contentGo.SetActive(true);
            MovePopup();
        }


    }
}
