using ExileCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RandomClick;

public class RandomClick : BaseSettingsPlugin<RandomClickSettings>
{
    private RectangleF _beastRectangle;
    private readonly List<RectangleF> _beastRectangles = new();
    private DateTime _lastClickTime = DateTime.MinValue;
    private readonly Random _random = new();
    private bool _isHoldingCtrl;
    private bool _isRunning;

    private const float LeftTrimPercent = 0.1f;
    private const float RightTrimPercent = 0.2f;
    private const int GridColumns = 3;
    private const int GridRows = 5;
    private const int ClickRow = 1;

    public override Job Tick()
    {
        HandleHotkey();

        if (!_isRunning)
            return null;

        if (!IsPanelVisible() || IsInventoryFull())
        {
            StopRunning();
            return null;
        }

        UpdateGrid();
        EnsureCtrlHeld();
        TryClick();

        return null;
    }

    private void HandleHotkey()
    {
        if (!Settings.OnOff.PressedOnce())
            return;

        if (_isRunning)
            StopRunning();
        else
            _isRunning = true;
    }

    private bool IsPanelVisible() =>
        GameController.IngameState.IngameUi.ChallengesPanel.IsVisible;

    private bool IsInventoryFull()
    {
        if (!Settings.InventoryOff.Value)
            return false;

        return CountItemsInventory() >= Settings.OffCount.Value;
    }

    private void StopRunning()
    {
        _isRunning = false;
        ReleaseCtrl();
    }

    private void EnsureCtrlHeld()
    {
        if (_isHoldingCtrl)
            return;

        Input.KeyDown(Keys.LControlKey);
        _isHoldingCtrl = true;
    }

    private void ReleaseCtrl()
    {
        if (!_isHoldingCtrl)
            return;

        Input.KeyUp(Keys.LControlKey);
        _isHoldingCtrl = false;
    }

    private void UpdateGrid()
    {
        var tabRect = GameController.IngameState.IngameUi.ChallengesPanel
            .TabContainer.GetClientRectCache;

        if (tabRect.IsEmpty)
            return;

        float leftOffset = tabRect.Width * LeftTrimPercent;
        float rightOffset = tabRect.Width * RightTrimPercent;

        _beastRectangle = new RectangleF(
            tabRect.X + leftOffset,
            tabRect.Y,
            tabRect.Width - leftOffset - rightOffset,
            tabRect.Height
        );

        BuildCells();
    }

    private void BuildCells()
    {
        _beastRectangles.Clear();

        float cellWidth = _beastRectangle.Width / GridColumns;
        float cellHeight = _beastRectangle.Height / GridRows;

        for (int col = 0; col < GridColumns; col++)
        {
            _beastRectangles.Add(new RectangleF(
                _beastRectangle.X + col * cellWidth,
                _beastRectangle.Y + ClickRow * cellHeight,
                cellWidth,
                cellHeight
            ));
        }
    }

    private void TryClick()
    {
        if ((DateTime.Now - _lastClickTime).TotalMilliseconds <= Settings.ActionDelay.Value)
            return;

        _lastClickTime = DateTime.Now;
        ClickRandomCell();
    }

    private void ClickRandomCell()
    {
        if (_beastRectangles.Count == 0)
            return;

        var windowRect = GameController.Window.GetWindowRectangleTimeCache;
        var cell = _beastRectangles[_random.Next(_beastRectangles.Count)];

        var center = new System.Numerics.Vector2(
            windowRect.X + cell.X + cell.Width / 2,
            windowRect.Y + cell.Y + cell.Height / 2
        );

        Input.SetCursorPos(center);
        Input.Click(MouseButtons.Left);
    }

    public int CountItemsInventory()
    {
        var inventory = GameController.IngameState.ServerData.PlayerInventories[0].Inventory;
        return inventory?.Items.Count ?? 0;
    }

    public override void Render()
    {
        if (_beastRectangle.IsEmpty)
            return;

        Graphics.DrawFrame(_beastRectangle, Color.Gray, 2);

        foreach (var rect in _beastRectangles)
            Graphics.DrawFrame(rect, Color.HotPink, 2);

        base.Render();
    }
}