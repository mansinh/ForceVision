using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PCControls : MonoBehaviour
{
    [SerializeField] float maxX, minX, maxZ, minZ;
    [SerializeField] float maxXPhoto, minXPhoto, maxYPhoto, minYPhoto;
    [SerializeField] float speed, rotateSpeed, speedPhoto;
    [SerializeField] Button nextMap;
    [SerializeField] Transform[] tableTops;
    [SerializeField] Transform photo;
    KeyCode rotateLeft = KeyCode.Q;
    KeyCode rotateRight = KeyCode.E;
    int tableTopIndex = 0;
    private void Start() {
        SelectTableTop();


        nextMap.onClick.AddListener(()=> {
            tableTopIndex++;
            tableTopIndex = tableTopIndex % tableTops.Length;
            SelectTableTop();
        });
    }
    void SelectTableTop() {
        for (int i = 0; i < tableTops.Length; i++) {
            tableTops[i].gameObject.SetActive(i == tableTopIndex);
        }
    }
    void Update()
    {
        Translation();
        Rotation();
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("space");
            //photo.gameObject.SetActive(!photo.gameObject.activeSelf);
        }
    }
    void Translation() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (photo.gameObject.activeSelf) {
            //MovePhoto(horizontal, vertical);
        } else {
            MoveTableTop(horizontal, vertical);
        }
    }
    void MovePhoto(float horizontal, float vertical) {
        float x = photo.localPosition.x;
        float y = photo.localPosition.y;
        y += vertical * speedPhoto * Time.deltaTime;
        x += horizontal * speedPhoto * Time.deltaTime;
        y = Mathf.Clamp(y, minYPhoto, maxYPhoto);
        x = Mathf.Clamp(x, minXPhoto, maxXPhoto);
        photo.localPosition = new Vector3(x, y, photo.localPosition.z);
    }
    void MoveTableTop(float horizontal, float vertical) {
        foreach (Transform tableTop in tableTops) {
   
            float x = tableTop.position.x;
            float z = tableTop.position.z;
            z -= vertical * speed * Time.deltaTime;
            x -= horizontal * speed * Time.deltaTime;
            z = Mathf.Clamp(z, minZ, maxZ);
            x = Mathf.Clamp(x, minX, maxX);
            tableTop.position = new Vector3(x, tableTop.position.y, z);
        }
    }
    void Rotation() {
        
        foreach (Transform tableTop in tableTops) {
          
            int rotate = 0;
            if (Input.GetKey(rotateLeft)) {
                rotate++;
            }
            if (Input.GetKey(rotateRight)) {
                rotate--;
            }
           // Debug.Log("rotation" + Vector3.up * rotate * rotateSpeed * Time.deltaTime);
            tableTop.eulerAngles += Vector3.up * rotate * rotateSpeed * Time.deltaTime;
        }
    }
}
