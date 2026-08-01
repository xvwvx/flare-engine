// <自动生成> 对应 C++ 源文件：Camera.h + Camera.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Camera
    ///
    /// 控制在地图上移动的视口，通常聚焦于玩家角色。
    /// </summary>
    public class Camera
    {
        public Camera()
        {
            Pos = default;
            Shake = default;
            _target = default;
            _prevCamTarget = default;
            _prevCamDx = 0;
            _prevCamDy = 0;
            _camThreshold = SharedResources.Eset!.Misc.CameraSpeed / Settings.LogicFps / 50f;
            _shakeStrength = 8;
        }

        // 对应 C++ 的 ~Camera()：原始析构函数体为空，Camera 不持有任何需要显式释放的资源，
        // 因此本单元无需实现 IDisposable，此处不生成对应方法（无逻辑可迁移）。

        public void Logic()
        {
            var eset = SharedResources.Eset!;

            // gradulally move camera towards target

            float camDelta = Utils.CalcDist(Pos, _target);
            float camDx = (Utils.CalcDist(new Vector2(Pos.X, _target.Y), _target)) / eset.Misc.CameraSpeed;
            float camDy = (Utils.CalcDist(new Vector2(_target.X, Pos.Y), _target)) / eset.Misc.CameraSpeed;

            if (_prevCamTarget.X == _target.X && _prevCamTarget.Y == _target.Y)
            {
                // target hasn't changed

                if (camDelta == 0 || camDelta >= _camThreshold)
                {
                    // camera is stationary or moving fast enough, so store the deltas
                    _prevCamDx = camDx;
                    _prevCamDy = camDy;
                }
                else if (camDelta < _camThreshold)
                {
                    if (camDx < _prevCamDx || camDy < _prevCamDy)
                    {
                        // maintain camera speed
                        camDx = _prevCamDx;
                        camDy = _prevCamDy;
                    }
                    else
                    {
                        // camera didn't get a chance to speed up, so set the minimum speed
                        float b = MathF.Abs(Pos.X - _target.X);
                        float alpha = MathF.Acos(b / camDelta);

                        float fastDx = _camThreshold * MathF.Cos(alpha);
                        float fastDy = _camThreshold * MathF.Sin(alpha);

                        _prevCamDx = fastDx / eset.Misc.CameraSpeed;
                        _prevCamDy = fastDy / eset.Misc.CameraSpeed;
                    }
                }
            }
            else
            {
                // target changed, reset
                _prevCamTarget = _target;
                _prevCamDx = 0;
                _prevCamDy = 0;
            }

            // camera movement might overshoot its target, so compensate for that here
            if (Pos.X < _target.X)
            {
                Pos.X += camDx;
                if (Pos.X > _target.X)
                    Pos.X = _target.X;
            }
            else if (Pos.X > _target.X)
            {
                Pos.X -= camDx;
                if (Pos.X < _target.X)
                    Pos.X = _target.X;
            }
            if (Pos.Y < _target.Y)
            {
                Pos.Y += camDy;
                if (Pos.Y > _target.Y)
                    Pos.Y = _target.Y;
            }
            else if (Pos.Y > _target.Y)
            {
                Pos.Y -= camDy;
                if (Pos.Y < _target.Y)
                    Pos.Y = _target.Y;
            }

            // handle camera shaking timer
            ShakeTimer.Tick();

            if (ShakeTimer.IsEnd())
            {
                Shake.X = Pos.X;
                Shake.Y = Pos.Y;
            }
            else
            {
                // 对应 C++ 的 rand()：按 main 单元的约定改为调用共享的 Program.Rng，
                // 以保留“全局共享同一随机数流”的语义（详见 main.report.txt 风险 4）。
                Shake.X = Pos.X + (float)((Program.Rng.Next(_shakeStrength * 2)) - _shakeStrength) * 0.0078125f;
                Shake.Y = Pos.Y + (float)((Program.Rng.Next(_shakeStrength * 2)) - _shakeStrength) * 0.0078125f;
            }
        }

        public void SetTarget(Vector2 target)
        {
            _target = target;
        }

        public void WarpTo(Vector2 target)
        {
            Pos = Shake = _target = _prevCamTarget = target;
            ShakeTimer.Reset(Timer.End);
            _prevCamDx = 0;
            _prevCamDy = 0;
        }

        public Vector2 Pos;
        public Vector2 Shake;
        public Timer ShakeTimer = new Timer();

        private Vector2 _target;
        private Vector2 _prevCamTarget;

        private float _prevCamDx;
        private float _prevCamDy;

        private readonly float _camThreshold;
        private int _shakeStrength;
    }
}
