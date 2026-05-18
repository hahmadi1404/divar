from flask import Flask, render_template_string, jsonify, request
import random
import copy

app = Flask(__name__)

# HTML/CSS/JS Template
HTML_TEMPLATE = '''
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>بازی 1024</title>
    <style>
        body {
            font-family: 'Tahoma', sans-serif;
            background-color: #faf8ef;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            margin: 0;
            user-select: none;
        }
        h1 {
            color: #776e65;
            font-size: 48px;
            margin: 10px 0;
        }
        .score-container {
            background: #bbada0;
            padding: 10px 20px;
            border-radius: 5px;
            color: white;
            font-size: 20px;
            margin-bottom: 20px;
        }
        .grid-container {
            background: #bbada0;
            padding: 15px;
            border-radius: 10px;
            display: grid;
            grid-template-columns: repeat(4, 100px);
            grid-template-rows: repeat(4, 100px);
            gap: 10px;
            position: relative;
        }
        .cell {
            background: #cdc1b4;
            border-radius: 5px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 36px;
            font-weight: bold;
            color: #776e65;
            transition: all 0.15s ease;
        }
        .cell-2 { background: #eee4da; }
        .cell-4 { background: #ede0c8; }
        .cell-8 { background: #f2b179; color: #f9f6f2; }
        .cell-16 { background: #f59563; color: #f9f6f2; }
        .cell-32 { background: #f67c5f; color: #f9f6f2; }
        .cell-64 { background: #f65e3b; color: #f9f6f2; }
        .cell-128 { background: #edcf72; color: #f9f6f2; font-size: 32px; }
        .cell-256 { background: #edcc61; color: #f9f6f2; font-size: 32px; }
        .cell-512 { background: #edc850; color: #f9f6f2; font-size: 32px; }
        .cell-1024 { background: #edc53f; color: #f9f6f2; font-size: 28px; }
        .cell-2048 { background: #edc22e; color: #f9f6f2; font-size: 28px; }
        .game-over {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(238, 228, 218, 0.73);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            border-radius: 10px;
            display: none;
        }
        .game-over.show {
            display: flex;
        }
        .game-over p {
            font-size: 48px;
            color: #776e65;
            font-weight: bold;
            margin: 0 0 20px 0;
        }
        button {
            background: #8f7a66;
            color: white;
            border: none;
            padding: 15px 30px;
            font-size: 18px;
            border-radius: 5px;
            cursor: pointer;
            font-family: 'Tahoma', sans-serif;
        }
        button:hover {
            background: #776e65;
        }
        .instructions {
            margin-top: 20px;
            color: #776e65;
            text-align: center;
        }
    </style>
</head>
<body>
    <h1>بازی 1024</h1>
    <div class="score-container">امتیاز: <span id="score">0</span></div>
    
    <div class="grid-container" id="grid">
        <div class="game-over" id="gameOver">
            <p>پایان بازی!</p>
            <button onclick="newGame()">بازی جدید</button>
        </div>
    </div>
    
    <button onclick="newGame()" style="margin-top: 20px;">بازی جدید</button>
    
    <div class="instructions">
        <p>از کلیدهای جهت‌نما (↑ ↓ ← →) برای حرکت استفاده کنید</p>
    </div>

    <script>
        let grid = [];
        
        function newGame() {
            fetch('/new_game', { method: 'POST' })
                .then(response => response.json())
                .then(data => {
                    grid = data.grid;
                    document.getElementById('score').textContent = data.score;
                    document.getElementById('gameOver').classList.remove('show');
                    renderGrid();
                });
        }
        
        function renderGrid() {
            const gridElement = document.getElementById('grid');
            const gameOverElement = document.getElementById('gameOver');
            
            // Keep game-over element, remove cells
            Array.from(gridElement.children).forEach(child => {
                if (!child.classList.contains('game-over')) {
                    gridElement.removeChild(child);
                }
            });
            
            for (let i = 0; i < 4; i++) {
                for (let j = 0; j < 4; j++) {
                    const cell = document.createElement('div');
                    cell.className = 'cell';
                    const value = grid[i][j];
                    if (value !== 0) {
                        cell.textContent = value;
                        cell.classList.add('cell-' + value);
                    }
                    gridElement.insertBefore(cell, gameOverElement);
                }
            }
        }
        
        document.addEventListener('keydown', function(event) {
            const keyMap = {
                'ArrowUp': 'up',
                'ArrowDown': 'down',
                'ArrowLeft': 'left',
                'ArrowRight': 'right'
            };
            
            if (keyMap[event.key]) {
                event.preventDefault();
                fetch('/move', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ direction: keyMap[event.key] })
                })
                .then(response => response.json())
                .then(data => {
                    if (data.grid) {
                        grid = data.grid;
                        document.getElementById('score').textContent = data.score;
                        renderGrid();
                        if (data.game_over) {
                            document.getElementById('gameOver').classList.add('show');
                        }
                    }
                });
            }
        });
        
        // Start new game on load
        newGame();
    </script>
</body>
</html>
'''

class Game1024:
    def __init__(self):
        self.grid = [[0] * 4 for _ in range(4)]
        self.score = 0
        self.add_new_tile()
        self.add_new_tile()
    
    def add_new_tile(self):
        empty_cells = [(i, j) for i in range(4) for j in range(4) if self.grid[i][j] == 0]
        if empty_cells:
            i, j = random.choice(empty_cells)
            self.grid[i][j] = 2 if random.random() < 0.9 else 4
    
    def compress(self, row):
        new_row = [val for val in row if val != 0]
        new_row += [0] * (4 - len(new_row))
        return new_row
    
    def merge(self, row):
        for i in range(3):
            if row[i] != 0 and row[i] == row[i + 1]:
                row[i] *= 2
                self.score += row[i]
                row[i + 1] = 0
        return row
    
    def reverse(self, row):
        return row[::-1]
    
    def transpose(self):
        self.grid = [list(row) for row in zip(*self.grid)]
    
    def move_left(self):
        changed = False
        for i in range(4):
            original = self.grid[i][:]
            self.grid[i] = self.compress(self.grid[i])
            self.grid[i] = self.merge(self.grid[i])
            self.grid[i] = self.compress(self.grid[i])
            if original != self.grid[i]:
                changed = True
        return changed
    
    def move_right(self):
        changed = False
        for i in range(4):
            original = self.grid[i][:]
            self.grid[i] = self.reverse(self.grid[i])
            self.grid[i] = self.compress(self.grid[i])
            self.grid[i] = self.merge(self.grid[i])
            self.grid[i] = self.compress(self.grid[i])
            self.grid[i] = self.reverse(self.grid[i])
            if original != self.grid[i]:
                changed = True
        return changed
    
    def move_up(self):
        self.transpose()
        changed = self.move_left()
        self.transpose()
        return changed
    
    def move_down(self):
        self.transpose()
        changed = self.move_right()
        self.transpose()
        return changed
    
    def is_game_over(self):
        # Check for empty cells
        for i in range(4):
            for j in range(4):
                if self.grid[i][j] == 0:
                    return False
        
        # Check for possible merges horizontally
        for i in range(4):
            for j in range(3):
                if self.grid[i][j] == self.grid[i][j + 1]:
                    return False
        
        # Check for possible merges vertically
        for i in range(3):
            for j in range(4):
                if self.grid[i][j] == self.grid[i + 1][j]:
                    return False
        
        return True

# Global game state
game_state = None

@app.route('/')
def index():
    return render_template_string(HTML_TEMPLATE)

@app.route('/new_game', methods=['POST'])
def new_game():
    global game_state
    game_state = Game1024()
    return jsonify({
        'grid': game_state.grid,
        'score': game_state.score
    })

@app.route('/move', methods=['POST'])
def move():
    global game_state
    if not game_state:
        game_state = Game1024()
    
    data = request.get_json()
    direction = data.get('direction')
    
    changed = False
    if direction == 'left':
        changed = game_state.move_left()
    elif direction == 'right':
        changed = game_state.move_right()
    elif direction == 'up':
        changed = game_state.move_up()
    elif direction == 'down':
        changed = game_state.move_down()
    
    if changed:
        game_state.add_new_tile()
    
    game_over = game_state.is_game_over()
    
    return jsonify({
        'grid': game_state.grid,
        'score': game_state.score,
        'game_over': game_over
    })

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=False)
