using System;
using Sharp2D;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;

namespace SomeGame
{
    public class SmoothCameraFollow : ILogical
    {
        private const float MaxVel = 3.3f;

        private float _xacc, _xvel, _negxacc;
        private float _yacc, _yvel, _negyacc;

        private float _lastX, _lastY;
        private Sprite _following;
        private World _world;

        private bool _xMoving;
        private bool _yMoving;
        private bool _moveXCamera;
        private bool _moveYCamera;

        public SmoothCameraFollow(Sprite following, World world)
        {
            _following = following;
            _world = world;

            following.Moved += following_Moved;
        }

        void following_Moved(object sender, OnMoveableMoved e)
        {
            float xdiff = e.Moveable.X - e.OldX;
            float ydiff = e.Moveable.Y - e.OldY;

            if (xdiff < 0)
            {
                _xacc = -0.3f;
                _negxacc = 0.3f;

                _xMoving = true;
                _moveXCamera = true;
            }
            else if (xdiff > 0)
            {
                _xacc = 0.3f;
                _negxacc = -0.3f;

                _xMoving = true;
                _moveXCamera = true;
            }

            if (ydiff < 0)
            {
                _yacc = -0.3f;
                _negyacc = 0.3f;

                _yMoving = true;
                _moveYCamera = true;
            }
            else if (ydiff > 0)
            {
                _yacc = 0.3f;
                _negyacc = -0.3f;

                _yMoving = true;
                _moveYCamera = true;
            }
        }

        public void Dispose()
        {
            _following = null;
            _world = null;
        }

        public void Update()
        {
            if (_xMoving)
            {
                if (_lastX == _following.X)
                    _xMoving = false;

                _lastX = _following.X;
            }

            if (_moveXCamera)
            {
                _xvel += _xMoving ? _xacc : _negxacc;

                _xvel = Math.Max(Math.Min(_xvel, MaxVel), -MaxVel);

                if (!_xMoving)
                {
                    if (Math.Abs(_xvel - 0f) < 0.2f || (_negxacc > 0 && _xvel > 0) || (_negxacc < 0 && _xvel < 0))
                    {
                        _moveXCamera = false;
                        _xvel = 0f;

                        if (Math.Abs(_following.X - -_world.Camera.X) > Screen.Settings.GameSize.Width/8f)
                        {
                            _world.Camera.PanTo(-_following.X, _following.Y, PanType.Smooth, 3000);
                        }
                    }
                }
            }


            if (_yMoving)
            {
                if (_lastY == _following.Y)
                    _yMoving = false;

                _lastY = _following.Y;
            }

            if (_moveYCamera)
            {
                _yvel += _yMoving ? _yacc : _negyacc;

                _yvel = Math.Max(Math.Min(_yvel, MaxVel), -MaxVel);

                if (!_yMoving)
                {
                    if (Math.Abs(_yvel - 0f) < 0.2f || (_negyacc > 0 && _yvel > 0) || (_negyacc < 0 && _yvel < 0))
                    {
                        _moveYCamera = false;
                        _yvel = 0f;

                        if (Math.Abs(_following.Y - _world.Camera.Y) > Screen.Settings.GameSize.Height / 8f)
                        {
                            _world.Camera.PanTo(-_following.X, _following.Y, PanType.Smooth, 3000);
                        }
                    }
                }
                
            }

            _world.Camera.X += -_xvel;
            _world.Camera.Y += _yvel;
        }
    }
}
