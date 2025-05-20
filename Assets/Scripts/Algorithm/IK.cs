using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK
{
    public Vector3 startBiasDir; // ��ʼ�ؽڵķ���ƫ��
    public Vector3 endBiasDir;   // �����ؽڵķ���ƫ��
    public bool useDirBias;       // ����Ƿ�ʹ�÷���ƫ��
    public float w1;              // ��ʼƫ���Ȩ��
    public float w2;              // ����ƫ���Ȩ��

    bool hasCalculatedLengths;    // ����Ƿ��Ѿ����������
    float[] lengths;              // �洢ÿ�������εĳ���
    float totalLength;            // ���й������ܳ���

    // ��������˶�ѧ��������
    public void Solve(Vector3[] positions, Vector3 target)
    {
        // �����û�м�������ȣ������ÿ�������ĳ���
        if (!hasCalculatedLengths)
        {
            hasCalculatedLengths = true;
            lengths = new float[positions.Length - 1];
            totalLength = 0;

            // ����ÿ�������εĳ����Լ��ܳ���
            for (int i = 0; i < positions.Length - 1; i++)
            {
                lengths[i] = (positions[i + 1] - positions[i]).magnitude;
                totalLength += lengths[i];
            }
        }

        // ���Ŀ���Ƿ񳬳��ɴﷶΧ
        bool targetOutOfReach = (target - positions[0]).magnitude >= totalLength;
        if (targetOutOfReach)
        {
            // ���������Χ�������йؽ�ֱ�����г���Ŀ��
            Vector3 dirToTarget = (target - positions[0]).normalized;
            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + dirToTarget * lengths[i - 1];
            }
        }
        else
        {
            // Ŀ����Դﵽ��ʹ��IK���
            SolvePass(positions, lengths, target, positions[0], true, 0);
        }
    }

    // ������̵ĵݹ鷽��
    void SolvePass(Vector3[] positions, float[] lengths, Vector3 anchor, Vector3 reachTarget, bool forwardPass, int iteration)
    {
        const int maxIterations = 100;  // ����������
        const float acceptableTargetError = 0.01f; // �ɽ��ܵ�Ŀ�����
        const float sqrAcceptableTargetError = acceptableTargetError * acceptableTargetError; // �ɽ�������ƽ��

        // ʹ��FABRIK��ǰ��ͺ��������˶�ѧ�����
        // ��ת�����Խ������ǰ��ͺ��򴫵ݣ�
        System.Array.Reverse(positions);
        System.Array.Reverse(lengths);

        positions[0] = anchor; // ����һ��λ������Ϊê��
        float iterationT = iteration / (maxIterations - 1f); // ���㵱ǰ�����ı���

        // ǰ�򴫵ݣ�����λ��
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 dir = (positions[i + 1] - positions[i]).normalized;

            if (useDirBias)
            {
                // Ӧ�÷���ƫ��
                float t = i / (positions.Length - 2f);
                float vacuumVertStr = Mathf.Exp(-t * w1) * Mathf.Exp(-iterationT * 10);
                float negVacuumVertStr = (1 - Mathf.Exp(-t * w2)) * Mathf.Exp(-iterationT * 10);
                Vector3 biasDir = (forwardPass) ? startBiasDir : endBiasDir;
                dir = (biasDir * vacuumVertStr + dir + negVacuumVertStr * Vector3.down).normalized;
            }

            // ����λ�ò�����y�᲻����0
            positions[i + 1] = positions[i] + dir * lengths[i];
            positions[i + 1].y = Mathf.Max(0, positions[i + 1].y);
        }

        // ����Ǻ��򴫵ݣ�����Ƿ�Ӧ��ֹ
        if (!forwardPass)
        {
            float sqrDstToTarget = (positions[positions.Length - 1] - reachTarget).sqrMagnitude;
            if (sqrDstToTarget <= sqrAcceptableTargetError)
            {
                // �ﵽ�ɽ��ܽ��
                return;
            }
            if (iteration >= maxIterations)
            {
                // �ﵽ����������
                return;
            }
        }

        // �ݹ���ã�������һ�δ���
        SolvePass(positions, lengths, reachTarget, anchor, !forwardPass, iteration + 1);
    }
}
