using System;

// ── Numeric constants ──
const int width = 50;
const int height = 20;
const int offsetX = 0;
const int offsetY = 3;
const int cellW = width / 2;
const int cellH = height / 2;
const int controlsLineOffset = 1;
const int endScreenOffset = 3;
const int exitPromptOffset = 8;

// ── Color constants ──
const ConsoleColor titleColor = ConsoleColor.Cyan;
const ConsoleColor wallColor = ConsoleColor.DarkGray;
const ConsoleColor playerColor = ConsoleColor.Yellow;
const ConsoleColor exitColor = ConsoleColor.Green;
const ConsoleColor corridorColor = ConsoleColor.DarkBlue;
const ConsoleColor controlsColor = ConsoleColor.DarkCyan;
const ConsoleColor winColor = ConsoleColor.Green;
const ConsoleColor loseColor = ConsoleColor.Red;

// ── String constants ──
const string titleBanner = """
    ╔══════════════════════════════════════════════════╗
    ║          🏃 ASCII MAZE  C#  🏃                   ║
    ╚══════════════════════════════════════════════════╝
    """;
const string controlsText = "  [Z/↑] Up   [S/↓] Down   [Q/←] Left   [D/→] Right   [Esc] Quit";
const string winMessage = """
      ╔════════════════════════════════╗
      ║   🎉  CONGRATULATIONS!  🎉     ║
      ║   You found the exit!          ║
      ╚════════════════════════════════╝
    """;
const string loseMessage = "\n  Game abandoned. See you soon!";
const string exitPrompt = "  Press any key to quit...";

// ── Main program ──
var grid = new CellType[width, height];
int playerX = 0, playerY = 0;

GenerateMaze(grid, playerX, playerY);
DrawGameScreen();

// ── Game loop ──
var won = false;

while (!won)
{
    var key = Console.ReadKey(true).Key;

    var nx = playerX;
    var ny = playerY;
    var quit = false;

    switch (key)
    {
        case ConsoleKey.Z:
        case ConsoleKey.UpArrow:    ny--; break;
        case ConsoleKey.S:
        case ConsoleKey.DownArrow:  ny++; break;
        case ConsoleKey.Q:
        case ConsoleKey.LeftArrow:  nx--; break;
        case ConsoleKey.D:
        case ConsoleKey.RightArrow: nx++; break;
        case ConsoleKey.Escape:     quit = true; break;
    }
    if (quit) break;

    if (nx >= 0 && nx < width && ny >= 0 && ny < height && grid[nx, ny] != CellType.Wall)
    {
        if (grid[nx, ny] == CellType.Exit) won = true;

        grid[playerX, playerY] = CellType.Corridor;
        DrawCell(playerX, playerY);

        playerX = nx;
        playerY = ny;
        grid[playerX, playerY] = CellType.Player;
        DrawCell(playerX, playerY);
    }
}

// ── End screen ──
if (won)
    DrawTextXY(0, offsetY + height + endScreenOffset, winMessage, winColor);
else
    DrawTextXY(0, offsetY + height + endScreenOffset, loseMessage, loseColor);

DrawTextXY(0, offsetY + height + exitPromptOffset, exitPrompt);
Console.CursorVisible = true;
Console.ReadKey(true);

// ── Functions ──
void DrawTextXY(int x, int y, string text, ConsoleColor? color = null)
{
    Console.SetCursorPosition(x, y);
    if (color.HasValue)
        Console.ForegroundColor = color.Value;
    Console.WriteLine(text);
    Console.ResetColor();
}

void DrawCell(int cx, int cy)
{
    Console.SetCursorPosition(offsetX + cx, offsetY + cy);
    var (color, pattern) = grid[cx, cy] switch
    {
        CellType.Wall   => (wallColor, "█"),
        CellType.Player => (playerColor, "@"),
        CellType.Exit   => (exitColor, "★"),
        _               => (corridorColor, "·")
    };
    Console.ForegroundColor = color;
    Console.Write(pattern);
    Console.ResetColor();
}

void DrawGameScreen()
{
    Console.Clear();
    Console.CursorVisible = false;

    DrawTextXY(0, 0, titleBanner, titleColor);

    for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            DrawCell(x, y);

    DrawTextXY(0, offsetY + height + controlsLineOffset, controlsText, controlsColor);
}

void GenerateMaze(CellType[,] maze, int startX, int startY)
{
    for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            maze[x, y] = CellType.Wall;

    var stackX = new int[cellW * cellH];
    var stackY = new int[cellW * cellH];
    var stackTop = 0;

    var visited = new bool[cellW, cellH];

    int[] dx = { 0, 1, 0, -1 };
    int[] dy = { -1, 0, 1, 0 };

    var rng = new Random();

    var startCX = startX / 2;
    var startCY = startY / 2;
    visited[startCX, startCY] = true;
    maze[startCX * 2, startCY * 2] = CellType.Corridor;

    stackX[stackTop] = startCX;
    stackY[stackTop] = startCY;
    stackTop++;

    while (stackTop > 0)
    {
        var cx = stackX[stackTop - 1];
        var cy = stackY[stackTop - 1];

        int[] directions = { 0, 1, 2, 3 };
        rng.Shuffle(directions);

        var found = false;
        foreach (var dir in directions)
        {
            var nx = cx + dx[dir];
            var ny = cy + dy[dir];
            if (nx >= 0 && nx < cellW && ny >= 0 && ny < cellH && !visited[nx, ny])
            {
                maze[cx * 2 + dx[dir], cy * 2 + dy[dir]] = CellType.Corridor;
                maze[nx * 2, ny * 2] = CellType.Corridor;
                visited[nx, ny] = true;
                stackX[stackTop] = nx;
                stackY[stackTop] = ny;
                stackTop++;
                found = true;
                break;
            }
        }
        if (!found) stackTop--;
    }

    maze[startX, startY] = CellType.Player;
    maze[(cellW - 1) * 2, (cellH - 1) * 2] = CellType.Exit;
}

// ── Types ──
enum CellType { Corridor, Wall, Player, Exit }
