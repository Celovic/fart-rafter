using System;
using UnityEngine;

namespace FartRafters
{
    [Serializable]
    public class SpectatorLook
    {
        public string name;
        public bool animal;
        public Mesh body;
        public Mesh head;
        public Mesh armLeft;
        public Mesh armRight;
        public Mesh eye;
        public Vector3 headPivot;
        public Vector3 shoulder;
        public Vector3 eyePosition;
    }
}
