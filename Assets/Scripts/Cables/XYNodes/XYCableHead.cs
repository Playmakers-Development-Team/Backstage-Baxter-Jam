﻿using UnityEngine;

namespace Cables
{
    public class XYCableHead : MonoBehaviour
    {
        [SerializeField] private CableHead cableHead;
        
        private CableXYNodeController xyNodeController;

        private void OnEnable()
        {
            cableHead.cableChanged.AddListener(OnCableChanged);
        }

        private void OnDisable()
        {
            cableHead.cableChanged.RemoveListener(OnCableChanged);
        }

        private void OnCableChanged()
        {
            xyNodeController = cableHead.Cable == null ? null : cableHead.Cable.GetComponent<CableXYNodeController>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            CheckPipeCollision(col);

            CheckCableCollision(col);
        }

        private void CheckCableCollision(Collider2D col)
        {
            if (cableHead.Cable == null) return;
            
            if (!col.CompareTag("Cable")) return;

            var hit = cableHead.TriggerCollision(cableHead.velocity);

            // Debug.Log("Player Cable Collision");
            // Debug.DrawLine(hit.point, hit.point + hit.normal, Color.yellow, 30f);

            GameEvents.PlayerCableCollision(hit.point, hit.normal);
        }

        private void CheckPipeCollision(Collider2D col)
        {
            // TODO: Duplicate code. See OnTriggerExit2D.
            if (cableHead.Cable == null) return;

            if (!col.CompareTag("Pipe")) return;

            var hit = cableHead.TriggerCollision(cableHead.velocity);

            Vector2 nodePosition = hit.point + hit.normal * cableHead.Cable.cableWidth / 2;

            // Draw collision normals
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.yellow, 30f);

            xyNodeController.PipeEnter(nodePosition, hit.normal);
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            // TODO: Duplicate code. See OnTriggerEnter2D.
            if (cableHead.Cable == null) return;
            
            if (!col.CompareTag("Pipe")) return;

            var hit = cableHead.TriggerCollision(-cableHead.velocity);
            
            xyNodeController.PipeExit(hit.normal);
        }
    }
}