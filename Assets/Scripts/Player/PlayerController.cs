﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlayerController : NetworkBehaviour {
    float speed = 20.0f;
    float gravity = 9.81f;
    float directionY;
    public string playerName = "";
    [SerializeField] GameObject prefab;
    Vector3 lookAt;
    Vector3 direction;
    CharacterController player;
    Rigidbody pickedItem = null;
    RaycastHit hit = new RaycastHit();

    [SerializeField] public TextMesh nameText;
    void Start() {
        nameText.text = playerName.Length > 0 ? playerName : "netID: " + GetComponent<NetworkIdentity>().netId;
        player = GetComponent<CharacterController>();
    }

    void Update() {
        if (!isLocalPlayer) return;
        nameText.transform.rotation = Camera.main.transform.rotation;
        Move();
        LookAtMouse();
        PickupItem();
        Jump();
        //Debug.Log(PrimitiveType.Plane);
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        player.Move(direction * Time.deltaTime * speed);
    }

    void Move() {
        direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        direction = Vector3.ClampMagnitude(direction, 1f);
        //transform.Translate(direction * Time.deltaTime * speed, Space.World);
        Camera.main.transform.position = transform.position + new Vector3(0, 10, -10);
    }

    void Jump() {
        if (Input.GetKeyDown(KeyCode.Space) && player.isGrounded) {
            //player.velocity += Vector3.up * 10f;
            directionY = 2f;
        }
        directionY -= gravity * Time.deltaTime;
        direction.y = directionY;
    }

    void LookAtMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane hPlane = new Plane(Vector3.up, -transform.position.y);
        float distance = 0;
        if (hPlane.Raycast(ray, out distance)) {
            lookAt = ray.GetPoint(distance);
            lookAt.y = transform.position.y;
        }
        //player.MoveRotation(Quaternion.LookRotation(lookAt, Vector3.up));
        transform.LookAt(lookAt);
    }

    [Command] //call from client, run in server
    void CmdSpawnPrefab() {
        //Debug.Log(transform.position);
        GameObject spawn = Instantiate(prefab, transform.position, transform.rotation);
        NetworkServer.Spawn(spawn, transform.gameObject);
        Debug.LogError("Spawn Success");
    }

    [Command]
    void CmdMoveItem() { //run server side movement.
        if (pickedItem != null) {
            pickedItem.MovePosition(transform.forward * 1.25f + transform.position);
            RpcMoveItem();
        }
    }

    [ClientRpc]
    void RpcMoveItem() { //run a client side approximation of server side movement.
        if (pickedItem != null) {
            pickedItem.MovePosition(transform.forward * 1.25f + transform.position);
        }
    }

    [Command]
    void CmdSetItem() {
        if (hit.rigidbody != null) {
            pickedItem = hit.rigidbody;
            //Debug.Log("ITEM: " + pickedItem);
        }
    }

    [Command]
    void CmdSphereCast() {
        Physics.SphereCast(transform.position, 0.5f, transform.TransformDirection(Vector3.forward), out hit, 4f);
    }

    void PickupItem() {
        CmdSphereCast();
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

        if (Input.GetMouseButtonDown(1)) {
            //Debug.LogError("Pressed Right Click");
            CmdSpawnPrefab();
        }

        if (Input.GetMouseButtonDown(0)) { //press e, ray cast
            //Debug.LogError("Pressed Left Click");
            CmdSetItem();
        }

        if (Input.GetMouseButton(0)) { //press and hold e
            //Debug.LogError("Pressed and Hold Left Click");
            CmdMoveItem();
        }

        if (Input.GetMouseButtonUp(0)) {
            //Debug.LogError("Released Left Click");
            pickedItem = null;
        }
    }
}
