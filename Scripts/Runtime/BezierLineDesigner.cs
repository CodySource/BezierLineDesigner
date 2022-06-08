using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodySource
{
    [ExecuteInEditMode()]
    public class BezierLineDesigner : MonoBehaviour
    {

        /// <summary>
        /// This script allows for the creation / editting of bezier curved paths for a line renderer
        /// </summary>

        #region PROPERTIES

        [Range(1, 500)] public int curveAccuracy = 20;
        public Transform _anchors = null;
        public LineRenderer line = null;

        [SerializeField] private bool _areHandlesHome = false;
        [SerializeField] private bool _mirrorHandleMovements = false;
        private Dictionary<string, Vector3> _cachedHandles;

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Uses the child anchors of the object to draw a bezier curve
        /// </summary>
        private void Update()
        {
            if (!enabled) return;
            if (_anchors == null) return;
            if (line == null) return;
            _ApplyHandleMirrors();
            if (_anchors.childCount <= 1) return;
            line.positionCount = curveAccuracy * (_anchors.childCount - 1);
            for (int c = 0; c < _anchors.childCount - 1; c++)
            {
                float t = 0f;
                Vector3 B = Vector3.zero;
                Vector3 p0 = _anchors.GetChild(c).position;
                Vector3 p1 = _anchors.GetChild(c).GetChild(0).position;
                Vector3 p2 = _anchors.GetChild(c).GetChild(1).position;
                Vector3 p3 = _anchors.GetChild(c + 1).position;
                Vector3[] points = Bezier.GetPoints(curveAccuracy, p0, p1, p2, p3);
                for (int i = 0; i < points.Length; i++) line.SetPosition((c * curveAccuracy) + i, points[i]);
            }
        }

        /// <summary>
        /// Draws the gizmos for finding handles in editor
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!enabled || _anchors == null) return;
            for (int i = 0; i < _anchors.childCount; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(_anchors.GetChild(i).position, Vector3.one * 0.5f);
                for (int c = 0; c < _anchors.GetChild(i).childCount; c++)
                {
                    if (i < _anchors.childCount - 1) Gizmos.DrawLine(_anchors.GetChild(i).GetChild(c).position, _anchors.GetChild((c % 2 == 0) ? i : i + 1).position);
                    if (i < _anchors.childCount - 1) Gizmos.DrawWireSphere(_anchors.GetChild(i).GetChild(c).position, 0.5f);
                }
            }
        }

        /// <summary>
        /// Applies mirroring for anchor handles
        /// </summary>
        private void _ApplyHandleMirrors()
        {
            if (_areHandlesHome)
            {
                for (int i = 0; i < _anchors.childCount; i++)
                {
                    _anchors.GetChild(i).GetChild(0).localPosition = Vector3.zero;
                    _anchors.GetChild(i).GetChild(1).position = (_anchors.childCount > i + 1) ? _anchors.GetChild(i + 1).position : _anchors.position;
                }
                _cachedHandles = new Dictionary<string, Vector3>();
                for (int i = 0; i < _anchors.childCount; i++)
                {
                    for (int c = 0; c < _anchors.GetChild(i).childCount; c++)
                    {
                        _cachedHandles.Add($"{i},{c}", _anchors.GetChild(i).GetChild(c).localPosition);
                    }
                }
                return;
            }
            if (!_mirrorHandleMovements) return;
            if (_cachedHandles == null)
            {
                _cachedHandles = new Dictionary<string, Vector3>();
                for (int i = 0; i < _anchors.childCount; i++)
                {
                    for (int c = 0; c < _anchors.GetChild(i).childCount; c++)
                    {
                        _cachedHandles.Add($"{i},{c}", _anchors.GetChild(i).GetChild(c).localPosition);
                    }
                }
            }
            else
            {
                string moved = "";
                Vector3 delta = Vector3.zero;
                foreach (KeyValuePair<string, Vector3> handle in _cachedHandles)
                {
                    Vector3 pos = _anchors.GetChild(int.Parse(handle.Key.Split(',')[0])).GetChild(int.Parse(handle.Key.Split(',')[1])).localPosition;
                    if (pos == handle.Value) continue;
                    delta = pos - handle.Value;
                    /// Update position
                    moved = handle.Key;
                    break;
                }
                if (moved == "") return;
                /// Find opposite handle
                string target = $"{((moved.Split(',')[1] == "0") ? int.Parse(moved.Split(',')[0]) - 1 : int.Parse(moved.Split(',')[0]) + 1)},{((moved.Split(',')[1] == "0") ? 1 : 0)}";
                if (_cachedHandles.ContainsKey(target))
                {
                    _anchors.GetChild(int.Parse(target.Split(',')[0])).GetChild(int.Parse(target.Split(',')[1])).localPosition -= delta;
                    _cachedHandles[moved] += delta;
                    _cachedHandles[target] -= delta;
                }
            }
        }

        #endregion

    }

    public class Bezier
    {

        public static Vector3[] GetPoints(int pAccuracy, Vector3 pPoint0, Vector3 pPoint1, Vector3 pPoint2, Vector3 pPoint3)
        {
            Vector3[] r = new Vector3[pAccuracy]; float t = 0f;
            Vector3 B = Vector3.zero;
            for (int i = 0; i < pAccuracy; i++)
            {
                B = (1 - t) * (1 - t) * (1 - t) * pPoint0 +
                    3 * (1 - t) * (1 - t) * t * pPoint1 +
                    3 * (1 - t) * t * t * pPoint2 +
                    t * t * t * pPoint3;
                r[i] = B;
                t += (1 / (float)pAccuracy);
            }
            return r;
        }

    }
}