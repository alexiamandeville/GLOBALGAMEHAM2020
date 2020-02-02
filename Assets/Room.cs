using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{

    public class Room : MonoBehaviour
    {
        ScroungeHotspot hotSpot;
        Vector3 roomSize;
        private int occupantCount;

        private void Start()
        {
            hotSpot = GetComponentInChildren<ScroungeHotspot>();
            roomSize = GetComponent<BoxCollider>().size / 2;
            PlaceHotspot();
        }

        private void PlaceHotspot()
        {
            Vector3 pos = transform.position;
            pos.x = pos.x + Random.Range(-roomSize.x, roomSize.x);
            pos.z = pos.z + Random.Range(-roomSize.z, roomSize.z);

            hotSpot.transform.position = pos;
            hotSpot.Reset();
        }

        public void OnPlayerEntered()
        {
            occupantCount++;
        }

        public void OnPlayerExited()
        {
            occupantCount--;
            if( occupantCount == 0 )
            {
                PlaceHotspot();
            }
        }
    }
}