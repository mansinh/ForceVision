using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;

namespace ForceVision
{
    [RequireComponent(typeof(AudioSource))]
    public class UIController : MonoBehaviour
    {

        [SerializeField] private TMP_InputField accuracyInput;
        [SerializeField] private TMP_InputField speedInput;
        [SerializeField] private Button inspectButton;
        [SerializeField] private Button detectButton;
        [SerializeField] private Button doorButton;
        [SerializeField] private Button crateButton;
        [SerializeField] private Button terminalButton;
        [SerializeField] private Toggle lineOfSightToggle;
        [SerializeField] private Toggle movementToggle;
        [SerializeField] private Toggle occupiedToggle;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private FadeInOut inspectPanel;
        [SerializeField] private GameObject doorGroup;
        [SerializeField] private GameObject crateGroup;
        [SerializeField] private GameObject terminalGroup;

        [SerializeField] CascadeDetector detector;
        [SerializeField] MapDisplay mapDisplay;
        private AudioSource audioSource;
        private bool isInspecting = false, isDetecting = false;
        private bool showingCrates = false, showingDoors = false, showingTerminals = false;
        private Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        private Color offColor = new Color(1, 1, 1, 0.2f);
        void Start()
        {
            mapDisplay = FindObjectOfType<MapDisplay>();
            detector = FindObjectOfType<CascadeDetector>();
            audioSource = GetComponent<AudioSource>();

            speedInput.text = $"{mapDisplay.Speed}";
            accuracyInput.text = $"{mapDisplay.Accuracy}";

            inspectButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            inspectPanel.TriggerFadeOut();

            InitializeButtons();
        }

        private void InitializeButtons()
        {
            inspectButton.onClick.AddListener(() => {
      
                SelectSound();
                inspectPanel.ToggleFade();
                if (isInspecting)
                {
                    inspectButton.GetComponent<Image>().color = offColor;
                    mapDisplay.DeselectAllCells();
                }
                else
                {
                    inspectButton.GetComponent<Image>().color = Color.white;

                    if (mapDisplay.IsCellSelected())
                        occupiedToggle.isOn = mapDisplay.GetCell(mapDisplay.selectedCellIndex).IsOccupied;
                }
                isInspecting = !isInspecting;
            });
            detectButton.GetComponent<Image>().color = offColor;
            detectButton.onClick.AddListener(() => {
                SelectSound();
                if (isDetecting)
                {
                    detectButton.GetComponent<Image>().color = offColor;
                    mapDisplay.ClearOccupied();
                }
                else
                {
                    detectButton.GetComponent<Image>().color = Color.white;
                    detector.Detect();
                    Debug.Log("detect");
                    foreach (OpenCVForUnity.CoreModule.Rect detected in detector.detectedRects)
                    {
                        Debug.Log("box");

                        Vector2 screenPosition = new Vector2(detected.x + (float)detected.size().width / 2, Screen.height - detected.y - (float)detected.size().height / 2);//new Vector2(detected.x+detected.width/2, Screen.height-detected.y-detected.height/2);
                        Debug.Log("DETECT"+screenPosition);
                        Debug.Log(detected.size().height + " " + detected.size().width);
                        Cell cell = mapDisplay.GetCellFromScreenPosition(screenPosition);
                        if (cell)
                            cell.IsOccupied = true;
                    }
                }
                isDetecting = !isDetecting;
            });

            doorGroup.transform.localScale = new Vector3(1, 1, 0.1f);
            doorButton.GetComponent<Image>().color = offColor;
            doorButton.onClick.AddListener(() => {
                SelectSound();
                showingDoors = !showingDoors;
                doorGroup.transform.localScale = showingDoors ? Vector3.one : new Vector3(1, 1, 0.1f);
                doorButton.GetComponent<Image>().color = showingDoors ? Color.white : offColor;
            });

            crateGroup.transform.localScale = new Vector3(1, 1, 0.1f);
            crateButton.GetComponent<Image>().color = offColor;
            crateButton.onClick.AddListener(() => {
                SelectSound();
                showingCrates = !showingCrates;
                crateGroup.transform.localScale = showingCrates ? Vector3.one : new Vector3(1, 1, 0.1f);
                crateButton.GetComponent<Image>().color = showingCrates ? Color.white : offColor;
            });

            terminalGroup.transform.localScale = new Vector3(1, 1, 0.1f);
            terminalButton.GetComponent<Image>().color = offColor;
            terminalButton.onClick.AddListener(() => {
                SelectSound();
                showingTerminals = !showingTerminals;
                terminalGroup.transform.localScale = showingTerminals ? Vector3.one : new Vector3(1, 1, 0.1f);
                terminalButton.GetComponent<Image>().color = showingTerminals ? Color.white : offColor;
            });


            lineOfSightToggle.onValueChanged.AddListener((isOn) => {
                SelectSound();
                mapDisplay.OnToggleLineOfSight(isOn);
            });
            movementToggle.onValueChanged.AddListener((isOn) => {
                SelectSound();
                mapDisplay.OnToggleMovement(isOn);
            });
            occupiedToggle.onValueChanged.AddListener((isOn) => {
                SelectSound();
                SetOccupied(isOn);
            });
        }
        public void SetOccupied(bool isOccupied)
        {
            Cell cell = mapDisplay.GetCell(mapDisplay.selectedCellIndex);
            if (cell)
            {
                cell.IsOccupied = isOccupied;
            }
        }
        public void SetOccupied(Vector3 screenPosition, bool isOccupied)
        {
            Cell cell = mapDisplay.GetCellFromScreenPosition(screenPosition);
            if (cell)
            {
                cell.IsOccupied = isOccupied;
            }
        }
        public void SetSpeed(string speed)
        {
            mapDisplay.SetAccuracy(speed);
        }
        public void SetAccuracy(string accuracy)
        {
            mapDisplay.SetAccuracy(accuracy);
        }
        private void SelectSound()
        {
            if (selectSound)
                audioSource.PlayOneShot(selectSound);
        }
        private void Update()
        {
            //Debug.Log(Input.mousePosition);
            if (!isInspecting)
                mapDisplay.SelectCellFromScreenPosition(screenCenter);

        }
    }
}
