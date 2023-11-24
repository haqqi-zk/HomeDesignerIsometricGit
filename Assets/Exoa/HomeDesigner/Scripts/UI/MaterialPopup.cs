using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Exoa.Designer
{
    public class MaterialPopup : BaseFloatingPopup
    {
        public static MaterialPopup Instance;
        private Renderer currentRenderer;
        private SpaceMaterialController room;
        private BuildingMaterialController building;
        private ModuleColorVariants module;
        private MaterialPopupUI wallSettings;

        public ModuleColorVariants Module { get => module; set => module = value; }
        public BuildingMaterialController Building { get => building; set => building = value; }
        public SpaceMaterialController Room { get => room; set => room = value; }

        public static UnityEvent OnClickColorButton = new UnityEvent();
        public Button colorButton;
        override protected void Awake()
        {
            Instance = this;
            wallSettings = GetComponentInChildren<MaterialPopupUI>(true);

            colorButton?.onClick.AddListener(() =>
            {
                OnClickColorButton.Invoke();
                Debug.Log("color button clicked");
                Hide();
            });

            base.Awake();
        }



        public void ShowMode(MaterialPopupUI.Mode mode, GameObject obj)
        {
            CurrentTarget = obj.transform;
            currentRenderer = obj.GetComponent<Renderer>();

            //print("ShowMode obj:" + obj);

            building = GameObject.FindObjectOfType<BuildingMaterialController>();
            room = obj.GetComponentInParent<SpaceMaterialController>();
            module = obj.GetComponentInChildren<ModuleColorVariants>();
            wallSettings.ShowMode(mode);

            Show();
            MovePopup();
        }

    }
}
