using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK
{
    public Vector3 startBiasDir; // 起始关节的方向偏差
    public Vector3 endBiasDir;   // 结束关节的方向偏差
    public bool useDirBias;       // 标记是否使用方向偏差
    public float w1;              // 起始偏差的权重
    public float w2;              // 结束偏差的权重

    bool hasCalculatedLengths;    // 标记是否已经计算过长度
    float[] lengths;              // 存储每个骨骼段的长度
    float totalLength;            // 所有骨骼的总长度

    // 解决逆向运动学的主方法
    public void Solve(Vector3[] positions, Vector3 target)
    {
        // 如果还没有计算过长度，则计算每个骨骼的长度
        if (!hasCalculatedLengths)
        {
            hasCalculatedLengths = true;
            lengths = new float[positions.Length - 1];
            totalLength = 0;

            // 计算每个骨骼段的长度以及总长度
            for (int i = 0; i < positions.Length - 1; i++)
            {
                lengths[i] = (positions[i + 1] - positions[i]).magnitude;
                totalLength += lengths[i];
            }
        }

        // 检查目标是否超出可达范围
        bool targetOutOfReach = (target - positions[0]).magnitude >= totalLength;
        if (targetOutOfReach)
        {
            // 如果超出范围，将所有关节直线排列朝向目标
            Vector3 dirToTarget = (target - positions[0]).normalized;
            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + dirToTarget * lengths[i - 1];
            }
        }
        else
        {
            // 目标可以达到，使用IK求解
            SolvePass(positions, lengths, target, positions[0], true, 0);
        }
    }

    // 解决过程的递归方法
    void SolvePass(Vector3[] positions, float[] lengths, Vector3 anchor, Vector3 reachTarget, bool forwardPass, int iteration)
    {
        const int maxIterations = 100;  // 最大迭代次数
        const float acceptableTargetError = 0.01f; // 可接受的目标误差
        const float sqrAcceptableTargetError = acceptableTargetError * acceptableTargetError; // 可接受误差的平方

        // 使用FABRIK（前向和后向逆向运动学）求解
        // 反转数组以交替进行前向和后向传递：
        System.Array.Reverse(positions);
        System.Array.Reverse(lengths);

        positions[0] = anchor; // 将第一个位置设置为锚点
        float iterationT = iteration / (maxIterations - 1f); // 计算当前迭代的比例

        // 前向传递，更新位置
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 dir = (positions[i + 1] - positions[i]).normalized;

            if (useDirBias)
            {
                // 应用方向偏差
                float t = i / (positions.Length - 2f);
                float vacuumVertStr = Mathf.Exp(-t * w1) * Mathf.Exp(-iterationT * 10);
                float negVacuumVertStr = (1 - Mathf.Exp(-t * w2)) * Mathf.Exp(-iterationT * 10);
                Vector3 biasDir = (forwardPass) ? startBiasDir : endBiasDir;
                dir = (biasDir * vacuumVertStr + dir + negVacuumVertStr * Vector3.down).normalized;
            }

            // 更新位置并限制y轴不低于0
            positions[i + 1] = positions[i] + dir * lengths[i];
            positions[i + 1].y = Mathf.Max(0, positions[i + 1].y);
        }

        // 如果是后向传递，检查是否应终止
        if (!forwardPass)
        {
            float sqrDstToTarget = (positions[positions.Length - 1] - reachTarget).sqrMagnitude;
            if (sqrDstToTarget <= sqrAcceptableTargetError)
            {
                // 达到可接受结果
                return;
            }
            if (iteration >= maxIterations)
            {
                // 达到最大迭代次数
                return;
            }
        }

        // 递归调用，进行下一次传递
        SolvePass(positions, lengths, reachTarget, anchor, !forwardPass, iteration + 1);
    }
}
