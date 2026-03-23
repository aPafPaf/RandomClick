using ExileCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RandomClick;

public class RandomClick : BaseSettingsPlugin<RandomClickSettings>
{
    private RectangleF _beastRectangle;
    private List<RectangleF> _beastRectangles = new();
    private DateTime _lastClickTime = DateTime.MinValue;
    private Random _random = new();
    private bool _isHoldingCtrl = false;
    private bool _isRunning = false;

    public override Job Tick()
    {
        var windowRect = GameController.Window.GetWindowRectangleTimeCache;

        // Переключение состояния цикла по горячей клавише
        if (Settings.OnOff.PressedOnce())
        {
            _isRunning = !_isRunning;

            // Если остановили - отпускаем Ctrl
            if (!_isRunning && _isHoldingCtrl)
            {
                Input.KeyUp(Keys.LControlKey);
                _isHoldingCtrl = false;
            }
        }

        // Если цикл не запущен - ничего не делаем
        if (!_isRunning)
            return null;

        // Проверяем видимость панели
        if (!GameController.IngameState.IngameUi.ChallengesPanel.IsVisible)
        {
            // Панель не видима - останавливаем цикл и отпускаем Ctrl
            _isRunning = false;
            if (_isHoldingCtrl)
            {
                Input.KeyUp(Keys.LControlKey);
                _isHoldingCtrl = false;
            }
            return null;
        }

        var rectangleTabContainer = GameController.IngameState.IngameUi.ChallengesPanel.TabContainer.GetClientRectCache;

        if (rectangleTabContainer.IsEmpty)
            return null;

        // Убрать 10% слева и 20% справа
        float leftOffset = rectangleTabContainer.Width * 0.1f;
        float rightOffset = rectangleTabContainer.Width * 0.2f;

        _beastRectangle = new RectangleF(
            rectangleTabContainer.X + leftOffset,
            rectangleTabContainer.Y,
            rectangleTabContainer.Width - leftOffset - rightOffset,
            rectangleTabContainer.Height
        );

        // Разделить на 3 вертикальных (колонки) и 5 горизонтальных (ряды) = 15 ячеек
        _beastRectangles.Clear();
        float cellWidth = _beastRectangle.Width / 3;
        float cellHeight = _beastRectangle.Height / 5;

        for (int row = 1; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var cell = new RectangleF(
                    _beastRectangle.X + col * cellWidth,
                    _beastRectangle.Y + row * cellHeight,
                    cellWidth,
                    cellHeight
                );
                _beastRectangles.Add(cell);
            }
        }

        // Зажать LCtrl пока цикл работает и панель видима
        if (!_isHoldingCtrl)
        {
            Input.KeyDown(Keys.LControlKey);
            _isHoldingCtrl = true;
        }

        // Автоматический клик по случайной ячейке
        if ((DateTime.Now - _lastClickTime).TotalMilliseconds > Settings.ActionDelay.Value)
        {
            _lastClickTime = DateTime.Now;
            ClickRandomCell(windowRect);
        }

        return null;
    }

    private void ClickRandomCell(RectangleF windowRect)
    {
        if (_beastRectangles.Count == 0)
            return;

        var randomCell = _beastRectangles[_random.Next(_beastRectangles.Count)];
        var center = new System.Numerics.Vector2(
            windowRect.X + randomCell.X + randomCell.Width / 2,
            windowRect.Y + randomCell.Y + randomCell.Height / 2
        );

        Input.SetCursorPos(center);
        Input.Click(MouseButtons.Left);
    }

    public override void Render()
    {
        if (_beastRectangle.IsEmpty)
            return;

        Graphics.DrawFrame(_beastRectangle, SharpDX.Color.Gray, 2);

        foreach (var rectangle in _beastRectangles)
        {
            Graphics.DrawFrame(rectangle, SharpDX.Color.HotPink, 2);
        }

        base.Render();
    }
}