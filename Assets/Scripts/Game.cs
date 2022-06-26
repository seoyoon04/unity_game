using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {
    public int size = 16;
    public int simCount = 40;
    public GameObject startAgain;

    private Board board;
    private Cell[,] state;
    private int[,] mineCount; // -1: Sim / 0~8: Sim count of near 8 cells
    
    // X = row, Y = column
    // X = row, Y = column
    // X = row, Y = column
    // X = row, Y = column

    private int[] dx = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };
    private int[] dy = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };

    private bool generated;
    private bool gameover;
    private int leftCellCount;

    public void OnClickRestart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOver() {
        startAgain.SetActive(true);
    }

    private void Awake() {
        board = GetComponentInChildren<Board>();
    }

    private void Start() {
        startAgain.SetActive(false);
        NewGame();
    }

    private void NewGame() {
        state = new Cell[size, size];
        mineCount = new int[size, size];
        generated = false;
        gameover = false;
        leftCellCount = size * size - simCount;

        GenerateCells();
        //GenerateSims(); -> generate Sim after first click to make the clicked cell free

        Camera.main.transform.position = new Vector3(size / 2f, size / 2f, -10f);
        board.Draw(state);
    }

    private void GenerateCells() {
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
                state[x, y].revealed = false;
                state[x, y].flagged = false;
                state[x, y].exploded = false;
            }
        }
    }

    private void GenerateSims() {
        generated = true;
        
        Grid grid = GetComponent<Grid>();

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        Vector3Int gridPos = grid.WorldToCell(world);
        int start_x = gridPos.x;
        int start_y = gridPos.y;

        for (int i = 0; i < simCount; i++) {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);

            while (state[x, y].type == Cell.Type.Mine || (abs(start_x - x) <= 1 && abs(start_y - y) <= 1)) {
                x++;

                if (x >= size) {
                    x = 0;
                    y++;

                    if (y >= size) {
                        y = 0;
                    }
                }
            }

            state[x, y].type = Cell.Type.Mine;
            // state[x, y].revealed = true;

            mineCount[x, y] = -1;
            for (int j = 0; j < 8; j++) {
                int nx = dx[j] + x;
                int ny = dy[j] + y;
                if (IsValidPos(nx, ny) && mineCount[nx, ny] != -1)
                    mineCount[nx, ny]++;
            }
        }

        for (int x = 0; x < size; x++){
            for (int y = 0; y < size; y++){
                if (mineCount[x, y] > 0) {
                    state[x, y].type = Cell.Type.Number;
                    state[x, y].number = mineCount[x, y];
                }
            }
        }
    }

    // private void GenerateNumber() {
        // useless function
    // }

    private void Update() {
        board.Draw(state);
        if (gameover) return;
        if (Input.GetMouseButtonDown(1)) {
            GenerateFlag();
        }
        else if (Input.GetMouseButtonDown(0)) {
            
            if (!generated) 
                GenerateSims();
            Reveal();
        }
    }

    private void GenerateFlag () {
        Grid grid = GetComponent<Grid>();

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        Vector3Int gridPos = grid.WorldToCell(world);
        int x = gridPos.x;
        int y = gridPos.y;

        if (!IsValidPos(x, y) || state[x, y].revealed) {
            return;
        } 

        if (state[x, y].flagged) state[x, y].flagged = false;
        else state[x, y].flagged = true;
    }

    private void Reveal() {

        Grid grid = GetComponent<Grid>();

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        Vector3Int gridPos = grid.WorldToCell(world);
        
        int x = gridPos.x;
        int y = gridPos.y;
        
        if (IsValidPos(x, y) && !state[x, y].flagged) {
            if (mineCount[x, y] == -1) {
                Explode();
            }
            else if (!state[x, y].revealed) {
                dfs(x, y);
            }
            else{
                Reveal_openedCell();
            }
        }

        Debug.Log(leftCellCount);
        if (leftCellCount == 0){
            Clear();
        }
    }

    private void Explode(){
        Debug.Log("Game over");
        gameover = true;
        for (int x = 0; x < size; x++){
            for (int y = 0; y < size; y++){
                if (state[x, y].type == Cell.Type.Mine)
                    state[x, y].revealed = true;
            }
        }
        GameOver();
    }

    private void Clear(){
        Debug.Log("clear");
        gameover = true;
    }

    private void Reveal_openedCell(){
        Grid grid = GetComponent<Grid>();

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        Vector3Int gridPos = grid.WorldToCell(world);
        
        int x = gridPos.x;
        int y = gridPos.y;

        if (!IsValidPos(x, y) || !state[x, y].revealed || state[x, y].flagged) return;

        int cnt = 0;
        bool wrongFlag = false;
        for (int i = 0; i < 8; i++){
            int nx = x + dx[i];
            int ny = y + dy[i];
            if (IsValidPos(nx, ny) && state[nx, ny].flagged) {
                cnt++;
                if (mineCount[nx, ny] != -1)
                    wrongFlag = true;
            }
        }
        if (cnt == mineCount[x, y]){
            if (wrongFlag){
                Explode();
                return;
            }
            for (int i = 0; i < 8; i++){
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (IsValidPos(nx, ny) && !state[nx, ny].flagged && !state[nx, ny].revealed) 
                    dfs(nx, ny);
            }
        }
    }

    private void dfs(int x, int y){
        state[x, y].revealed = true;
        state[x, y].flagged = false;
        leftCellCount--;
        if (mineCount[x, y] != 0) 
            return;
        for (int i = 0; i < 8; i++){
            int nx = x + dx[i];
            int ny = y + dy[i];
            if (IsValidPos(nx, ny) && !state[nx, ny].revealed)
                dfs(nx, ny);
        }
    }

    private bool IsValidPos(int x, int y){
        return 0 <= x && x < size && 0 <= y && y < size;
    }

    private int abs(int n){
        if (n >= 0) return n;
        return -n;
    }
}