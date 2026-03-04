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

// ── Maze generation with "recursive backtracker" ──
var grid = new CellType[width, height];

for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
        grid[x, y] = CellType.Wall;

var stackX = new int[cellW * cellH];
var stackY = new int[cellW * cellH];
var stackTop = 0;

var visited = new bool[cellW, cellH];

int[] dx = { 0, 1, 0, -1 };
int[] dy = { -1, 0, 1, 0 };

var rng = new Random();

int startCX = 0, startCY = 0;
visited[startCX, startCY] = true;
grid[startCX * 2, startCY * 2] = CellType.Corridor;

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
            grid[cx * 2 + dx[dir], cy * 2 + dy[dir]] = CellType.Corridor;
            grid[nx * 2, ny * 2] = CellType.Corridor;
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

// ── Player and exit positions ──
int playerX = 0, playerY = 0;
var exitX = (cellW - 1) * 2;
var exitY = (cellH - 1) * 2;

grid[playerX, playerY] = CellType.Player;
grid[exitX, exitY] = CellType.Exit;

// ── Initial full draw ──
Console.Clear();
Console.CursorVisible = false;

Console.SetCursorPosition(0, 0);
Console.ForegroundColor = titleColor;
Console.WriteLine(titleBanner);
Console.ResetColor();

for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
        DrawCell(x, y);

Console.SetCursorPosition(0, offsetY + height + controlsLineOffset);
Console.ForegroundColor = controlsColor;
Console.Write(controlsText);
Console.ResetColor();

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

// ── Game loop ──
var won = false;

while (!won)
{
    var key = Console.ReadKey(true).Key;

    var nx2 = playerX;
    var ny2 = playerY;
    var quit = false;

    switch (key)
    {
        case ConsoleKey.Z:
        case ConsoleKey.UpArrow:    ny2--; break;
        case ConsoleKey.S:
        case ConsoleKey.DownArrow:  ny2++; break;
        case ConsoleKey.Q:
        case ConsoleKey.LeftArrow:  nx2--; break;
        case ConsoleKey.D:
        case ConsoleKey.RightArrow: nx2++; break;
        case ConsoleKey.Escape:     quit = true; break;
    }
    if (quit) break;

    if (nx2 >= 0 && nx2 < width && ny2 >= 0 && ny2 < height && grid[nx2, ny2] != CellType.Wall)
    {
        if (grid[nx2, ny2] == CellType.Exit) won = true;

        grid[playerX, playerY] = CellType.Corridor;
        DrawCell(playerX, playerY);

        playerX = nx2;
        playerY = ny2;
        grid[playerX, playerY] = CellType.Player;
        DrawCell(playerX, playerY);
    }
}

// ── End screen ──
Console.SetCursorPosition(0, offsetY + height + endScreenOffset);
if (won)
{
    Console.ForegroundColor = winColor;
    Console.WriteLine(winMessage);
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = loseColor;
    Console.WriteLine(loseMessage);
    Console.ResetColor();
}

Console.SetCursorPosition(0, offsetY + height + exitPromptOffset);
Console.WriteLine(exitPrompt);
Console.CursorVisible = true;
Console.ReadKey(true);

// ── Types ──
enum CellType { Corridor, Wall, Player, Exit }
