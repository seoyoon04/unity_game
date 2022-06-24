using UnityEngine;

public class Game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int simCount = 32;

    private Board board;
    private Cell[,] state;

    private void Awake() {
        board = GetComponentInChildren<Board>();
    }

    private void Start() {
        NewGame();
    }

    private void NewGame() {
        state = new Cell[width, height];

        GenerateCells();
        GenerateSims();
        
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        board.Draw(state);
    }

    private void GenerateCells() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;

            }
        }
    }

    private void GenerateSims() {
        for (int i = 0; i < simCount; i++) {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (state[x, y].type == Cell.Type.Mine) {
                x++;

                if (x >= width) {
                    x = 0;
                    y++;

                    if (y >= height) {
                        y = 0;
                    }
                }
            }

            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumber() {

    }

    private void GenerateFlag () {

    }

}
