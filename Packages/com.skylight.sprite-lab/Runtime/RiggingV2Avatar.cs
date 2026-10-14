using UnityEngine;

namespace SpriteLab.RiggingV2
{
    public sealed class RiggingV2Avatar : MonoBehaviour
    {
        [Header("Automatic idle preview")]
        [SerializeField] private bool automaticIdle = true;
        [SerializeField, Range(0f, 3f)] private float bodySwayDegrees = 0.65f;
        [SerializeField, Range(0f, 3f)] private float headFollowDegrees = 0.45f;
        [SerializeField, Range(0f, 0.03f)] private float breathingScale = 0.008f;
        [SerializeField, Range(0.1f, 3f)] private float idleSpeed = 0.8f;
        [Tooltip("상체(spine 아래)가 위아래로 오르내리는 폭 (월드 단위, 100px = 1)")]
        [SerializeField, Range(0f, 0.2f)] private float bodyBob;

        [SerializeField] private Transform hips;
        [SerializeField] private Transform spine;
        [SerializeField] private Transform chest;
        [SerializeField] private Transform neck;
        [SerializeField] private Transform head;
        [SerializeField] private Transform skirtCenter;
        [SerializeField] private Transform skirtLeft;
        [SerializeField] private Transform skirtRight;
        [SerializeField] private Transform legLeft;
        [SerializeField] private Transform legRight;

        [Header("Arms (비워 두면 이름으로 자동 연결: shoulder_l/r, arm_l/r, hand_l/r)")]
        [Tooltip("팔이 숨쉬기·몸통에 맞춰 흔들리는 폭. 0이면 팔은 가만히 있다")]
        [SerializeField, Range(0f, 6f)] private float armSwayDegrees;
        [SerializeField] private Transform shoulderLeft;
        [SerializeField] private Transform shoulderRight;
        [SerializeField] private Transform armLeft;
        [SerializeField] private Transform armRight;
        [SerializeField] private Transform handLeft;
        [SerializeField] private Transform handRight;

        private Quaternion spineRest;
        private Vector3 spinePosition;
        private Quaternion chestRest;
        private Quaternion neckRest;
        private Quaternion headRest;
        private Vector3 chestScale;
        private Quaternion skirtCenterRest;
        private Quaternion skirtLeftRest;
        private Quaternion skirtRightRest;
        private Quaternion legLeftRest;
        private Quaternion legRightRest;
        private Quaternion shoulderLeftRest, shoulderRightRest, armLeftRest, armRightRest, handLeftRest, handRightRest;

        public void Configure(Transform hipsBone, Transform spineBone, Transform chestBone, Transform neckBone, Transform headBone,
            Transform skirtCenterBone = null, Transform skirtLeftBone = null, Transform skirtRightBone = null,
            Transform legLeftBone = null, Transform legRightBone = null)
        {
            hips = hipsBone;
            spine = spineBone;
            chest = chestBone;
            neck = neckBone;
            head = headBone;
            skirtCenter = skirtCenterBone;
            skirtLeft = skirtLeftBone;
            skirtRight = skirtRightBone;
            legLeft = legLeftBone;
            legRight = legRightBone;
            CachePose();
        }

        public void ApplyIdleSettings(bool enabled, float bodySway, float headFollow, float breathing, float speed, float bob = 0f, float armSway = 0f)
        {
            armSwayDegrees = Mathf.Max(0f, armSway);
            bodyBob = Mathf.Max(0f, bob);
            automaticIdle = enabled;
            bodySwayDegrees = Mathf.Max(0f, bodySway);
            headFollowDegrees = Mathf.Max(0f, headFollow);
            breathingScale = Mathf.Max(0f, breathing);
            idleSpeed = Mathf.Max(0.01f, speed);
        }

        private void Awake()
        {
            FindArmBones();
            CachePose();
        }

        // 임포터가 팔 뼈를 연결하지 않으므로 이름으로 찾는다 (이미 연결돼 있으면 그대로)
        private void FindArmBones()
        {
            Transform Find(string boneName)
            {
                foreach (var t in GetComponentsInChildren<Transform>(true))
                    if (t.name == boneName) return t;
                return null;
            }
            if (!shoulderLeft) shoulderLeft = Find("shoulder_l");
            if (!shoulderRight) shoulderRight = Find("shoulder_r");
            if (!armLeft) armLeft = Find("arm_l");
            if (!armRight) armRight = Find("arm_r");
            if (!handLeft) handLeft = Find("hand_l");
            if (!handRight) handRight = Find("hand_r");
        }

        private void CachePose()
        {
            shoulderLeftRest = shoulderLeft ? shoulderLeft.localRotation : Quaternion.identity;
            shoulderRightRest = shoulderRight ? shoulderRight.localRotation : Quaternion.identity;
            armLeftRest = armLeft ? armLeft.localRotation : Quaternion.identity;
            armRightRest = armRight ? armRight.localRotation : Quaternion.identity;
            handLeftRest = handLeft ? handLeft.localRotation : Quaternion.identity;
            handRightRest = handRight ? handRight.localRotation : Quaternion.identity;
            spineRest = spine ? spine.localRotation : Quaternion.identity;
            spinePosition = spine ? spine.localPosition : Vector3.zero;
            chestRest = chest ? chest.localRotation : Quaternion.identity;
            neckRest = neck ? neck.localRotation : Quaternion.identity;
            headRest = head ? head.localRotation : Quaternion.identity;
            chestScale = chest ? chest.localScale : Vector3.one;
            skirtCenterRest = skirtCenter ? skirtCenter.localRotation : Quaternion.identity;
            skirtLeftRest = skirtLeft ? skirtLeft.localRotation : Quaternion.identity;
            skirtRightRest = skirtRight ? skirtRight.localRotation : Quaternion.identity;
            legLeftRest = legLeft ? legLeft.localRotation : Quaternion.identity;
            legRightRest = legRight ? legRight.localRotation : Quaternion.identity;
        }

        private void Update()
        {
            if (!automaticIdle) return;
            var phase = Time.time * idleSpeed;
            var sway = Mathf.Sin(phase) * bodySwayDegrees;
            var breath = (Mathf.Sin(phase * 1.7f - 0.8f) + 1f) * 0.5f;
            if (spine)
            {
                spine.localRotation = spineRest * Quaternion.Euler(0f, 0f, sway * 0.45f);
                // 숨쉬기 리듬에 맞춰 상체가 살짝 오르내린다
                spine.localPosition = spinePosition + new Vector3(0f, (breath - 0.5f) * 2f * bodyBob, 0f);
            }
            if (chest)
            {
                chest.localRotation = chestRest * Quaternion.Euler(0f, 0f, sway);
                chest.localScale = chestScale + new Vector3(breathingScale * breath * 0.35f, breathingScale * breath, 0f);
            }
            if (neck) neck.localRotation = neckRest * Quaternion.Euler(0f, 0f, -sway * 0.55f);
            if (head) head.localRotation = headRest * Quaternion.Euler(0f, 0f, -Mathf.Sin(phase + 0.35f) * headFollowDegrees);
            var skirtSway = Mathf.Sin(phase * 0.92f + 1.1f);
            if (skirtCenter) skirtCenter.localRotation = skirtCenterRest * Quaternion.Euler(0f, 0f, skirtSway * 0.55f);
            if (skirtLeft) skirtLeft.localRotation = skirtLeftRest * Quaternion.Euler(0f, 0f, skirtSway * 0.85f);
            if (skirtRight) skirtRight.localRotation = skirtRightRest * Quaternion.Euler(0f, 0f, -skirtSway * 0.85f);
            var step = Mathf.Sin(phase * 0.72f + 0.2f) * 0.8f;
            if (legLeft) legLeft.localRotation = legLeftRest * Quaternion.Euler(0f, 0f, step);
            if (legRight) legRight.localRotation = legRightRest * Quaternion.Euler(0f, 0f, -step);

            // 팔: 숨쉬기에 어깨가 살짝 들리고, 팔은 몸통보다 조금 늦게 따라 흔들리며, 손끝은 더 늦게 살랑인다
            if (armSwayDegrees <= 0f) return;
            float shoulder = (breath - 0.5f) * armSwayDegrees * 0.5f;
            float arm = Mathf.Sin(phase - 0.6f) * armSwayDegrees;
            float hand = Mathf.Sin(phase * 1.3f - 1.4f) * armSwayDegrees * 1.2f;
            if (shoulderLeft) shoulderLeft.localRotation = shoulderLeftRest * Quaternion.Euler(0f, 0f, shoulder);
            if (shoulderRight) shoulderRight.localRotation = shoulderRightRest * Quaternion.Euler(0f, 0f, -shoulder);
            if (armLeft) armLeft.localRotation = armLeftRest * Quaternion.Euler(0f, 0f, arm);
            if (armRight) armRight.localRotation = armRightRest * Quaternion.Euler(0f, 0f, arm * 0.8f);
            if (handLeft) handLeft.localRotation = handLeftRest * Quaternion.Euler(0f, 0f, hand);
            if (handRight) handRight.localRotation = handRightRest * Quaternion.Euler(0f, 0f, -hand);
        }
    }
}
