using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCControls : MonoBehaviour
{
    [SerializeField] float maxX, minX, maxZ, minZ;
    [SerializeField] float maxXPhoto, minXPhoto, maxYPhoto, minYPhoto;
    [SerializeField] float speed, rotateSpeed, speedPhoto;
    [SerializeField] Transform tableTop, photo;
    KeyCode rotateLeft = KeyCode.Q;
    KeyCode rotateRight = KeyCode.E;
    
    void Update()
    {
        Translation();
        Rotation();
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("space");
            photo.gameObject.SetActive(!photo.gameObject.activeSelf);
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
        float x = tableTop.position.x;
        float z = tableTop.position.z;
        z -= vertical * speed * Time.deltaTime;
        x -= horizontal * speed * Time.deltaTime;
        z = Mathf.Clamp(z, minZ, maxZ);
        x = Mathf.Clamp(x, minX, maxX);
        tableTop.position = new Vector3(x, tableTop.position.y, z);
    }
    void Rotation() {
        if (tableTop.gameObject.activeSelf) return;
        int rotate = 0;
        if (Input.GetKey(rotateLeft)) {
            rotate++;
        }
        if (Input.GetKey(rotateRight)) {
            rotate--;
        }

        tableTop.eulerAngles += Vector3.up * rotate * rotateSpeed * Time.deltaTime;
    }
}
