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
    <title>بازی 2048</title>
    <link href="https://fonts.googleapis.com/css2?family=Vazirmatn:wght@400;700;900&display=swap" rel="stylesheet">
    <style>
        * {
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Vazirmatn', 'Tahoma', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            margin: 0;
            user-select: none;
            padding: 20px;
        }
        
        h1 {
            color: #fff;
            font-size: 64px;
            margin: 10px 0;
            text-shadow: 0 4px 8px rgba(0,0,0,0.3);
            font-weight: 900;
            letter-spacing: 2px;
        }
        
        .header-container {
            display: flex;
            gap: 20px;
            align-items: center;
            margin-bottom: 20px;
        }
        
        .score-container {
            background: linear-gradient(135deg, #ff6b6b, #ee5a6f);
            padding: 15px 30px;
            border-radius: 15px;
            color: white;
            font-size: 24px;
            box-shadow: 0 8px 20px rgba(0,0,0,0.2);
            transition: transform 0.2s;
        }
        
        .score-container.score-increased {
            transform: scale(1.1);
        }
        
        .best-score {
            background: linear-gradient(135deg, #4facfe, #00f2fe);
            padding: 15px 30px;
            border-radius: 15px;
            color: white;
            font-size: 24px;
            box-shadow: 0 8px 20px rgba(0,0,0,0.2);
        }
        
        .grid-container {
            background: linear-gradient(135deg, #3d3d3d, #1a1a1a);
            padding: 20px;
            border-radius: 20px;
            display: grid;
            grid-template-columns: repeat(4, 110px);
            grid-template-rows: repeat(4, 110px);
            gap: 15px;
            position: relative;
            box-shadow: 0 20px 60px rgba(0,0,0,0.4), inset 0 2px 10px rgba(255,255,255,0.1);
        }
        
        .cell {
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
            font-weight: bold;
            transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
            animation: pop 0.2s ease-out;
        }
        
        @keyframes pop {
            0% { transform: scale(0); }
            50% { transform: scale(1.1); }
            100% { transform: scale(1); }
        }
        
        @keyframes merge {
            0% { transform: scale(1); }
            50% { transform: scale(1.15); }
            100% { transform: scale(1); }
        }
        
        .cell.merged {
            animation: merge 0.3s ease-out;
        }
        
        .cell-empty {
            background: rgba(255,255,255,0.1);
            box-shadow: inset 0 2px 5px rgba(0,0,0,0.1);
        }
        
        .cell-2 { 
            background: linear-gradient(135deg, #a8edea, #fed6e3);
            color: #3d3d3d;
        }
        
        .cell-4 { 
            background: linear-gradient(135deg, #fdcbf1, #e6dee9);
            color: #3d3d3d;
        }
        
        .cell-8 { 
            background: linear-gradient(135deg, #f093fb, #f5576c);
            color: #fff;
        }
        
        .cell-16 { 
            background: linear-gradient(135deg, #4facfe, #00f2fe);
            color: #fff;
        }
        
        .cell-32 { 
            background: linear-gradient(135deg, #43e97b, #38f9d7);
            color: #fff;
        }
        
        .cell-64 { 
            background: linear-gradient(135deg, #fa709a, #fee140);
            color: #fff;
        }
        
        .cell-128 { 
            background: linear-gradient(135deg, #a18cd1, #fbc2eb);
            color: #fff;
            font-size: 36px;
            box-shadow: 0 0 30px rgba(161,140,209,0.6);
        }
        
        .cell-256 { 
            background: linear-gradient(135deg, #ff9a9e, #fecfef);
            color: #fff;
            font-size: 36px;
            box-shadow: 0 0 30px rgba(255,154,158,0.6);
        }
        
        .cell-512 { 
            background: linear-gradient(135deg, #ffecd2, #fcb69f);
            color: #fff;
            font-size: 36px;
            box-shadow: 0 0 30px rgba(252,182,159,0.6);
        }
        
        .cell-1024 { 
            background: linear-gradient(135deg, #f6d365, #fda085);
            color: #fff;
            font-size: 32px;
            box-shadow: 0 0 40px rgba(246,211,101,0.8);
        }
        
        .cell-2048 { 
            background: linear-gradient(135deg, #ff0844, #ffb199);
            color: #fff;
            font-size: 32px;
            box-shadow: 0 0 50px rgba(255,8,68,0.8);
        }
        
        .cell-super { 
            background: linear-gradient(135deg, #667eea, #764ba2);
            color: #fff;
            font-size: 28px;
            box-shadow: 0 0 60px rgba(102,126,234,0.9);
        }
        
        .game-over {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.85);
            backdrop-filter: blur(10px);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            border-radius: 20px;
            display: none;
            z-index: 10;
        }
        
        .game-over.show {
            display: flex;
            animation: fadeIn 0.5s ease-out;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }
        
        .game-over p {
            font-size: 56px;
            color: #fff;
            font-weight: 900;
            margin: 0 0 30px 0;
            text-shadow: 0 0 30px rgba(255,255,255,0.5);
        }
        
        button {
            background: linear-gradient(135deg, #667eea, #764ba2);
            color: white;
            border: none;
            padding: 18px 40px;
            font-size: 22px;
            border-radius: 15px;
            cursor: pointer;
            font-family: 'Vazirmatn', sans-serif;
            font-weight: 700;
            box-shadow: 0 8px 20px rgba(102,126,234,0.4);
            transition: all 0.3s;
        }
        
        button:hover {
            transform: translateY(-3px);
            box-shadow: 0 12px 30px rgba(102,126,234,0.6);
        }
        
        button:active {
            transform: translateY(-1px);
        }
        
        .instructions {
            margin-top: 30px;
            color: rgba(255,255,255,0.9);
            text-align: center;
            font-size: 18px;
            background: rgba(255,255,255,0.1);
            padding: 20px 30px;
            border-radius: 15px;
            backdrop-filter: blur(10px);
        }
        
        .controls-hint {
            display: flex;
            gap: 10px;
            justify-content: center;
            margin-top: 15px;
        }
        
        .key {
            background: rgba(255,255,255,0.2);
            padding: 8px 15px;
            border-radius: 8px;
            font-size: 16px;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <h1>🎮 2048</h1>
    
    <div class="header-container">
        <div class="score-container" id="scoreContainer">امتیاز: <span id="score">0</span></div>
        <div class="best-score">بهترین: <span id="bestScore">0</span></div>
    </div>
    
    <div class="grid-container" id="grid">
        <div class="game-over" id="gameOver">
            <p>پایان بازی! 😔</p>
            <button onclick="newGame()">بازی جدید 🔄</button>
        </div>
    </div>
    
    <button onclick="newGame()" style="margin-top: 25px;">بازی جدید 🎲</button>
    
    <div class="instructions">
        <p>از کلیدهای جهت‌نما برای حرکت استفاده کنید</p>
        <div class="controls-hint">
            <span class="key">↑</span>
            <span class="key">↓</span>
            <span class="key">←</span>
            <span class="key">→</span>
        </div>
    </div>

    <script>
        let grid = [];
        let bestScore = localStorage.getItem('bestScore') || 0;
        document.getElementById('bestScore').textContent = bestScore;
        
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
                    const value = grid[i][j];
                    if (value === 0) {
                        cell.className = 'cell cell-empty';
                    } else {
                        if (value <= 2048) {
                            cell.className = 'cell cell-' + value;
                        } else {
                            cell.className = 'cell cell-super';
                        }
                        cell.textContent = value;
                    }
                    gridElement.insertBefore(cell, gameOverElement);
                }
            }
        }
        
        document.addEventListener('keydown', function(event) {
            // Map arrow keys to directions - handle RTL properly
            const keyMap = {
                'ArrowUp': 'up',
                'ArrowDown': 'down',
                'ArrowLeft': 'right',  // In RTL, left arrow visually goes right
                'ArrowRight': 'left'   // In RTL, right arrow visually goes left
            };
            
            // Also support WASD keys
            const wasdMap = {
                'w': 'up',
                'W': 'up',
                's': 'down',
                'S': 'down',
                'a': 'left',
                'A': 'left',
                'd': 'right',
                'D': 'right'
            };
            
            let direction = null;
            
            if (keyMap[event.key]) {
                direction = keyMap[event.key];
            } else if (wasdMap[event.key]) {
                direction = wasdMap[event.key];
            }
            
            if (direction) {
                event.preventDefault();
                fetch('/move', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ direction: direction })
                })
                .then(response => response.json())
                .then(data => {
                    if (data.grid) {
                        const oldScore = parseInt(document.getElementById('score').textContent);
                        grid = data.grid;
                        document.getElementById('score').textContent = data.score;
                        
                        // Update best score
                        if (data.score > bestScore) {
                            bestScore = data.score;
                            localStorage.setItem('bestScore', bestScore);
                            document.getElementById('bestScore').textContent = bestScore;
                        }
                        
                        // Score increase animation
                        if (data.score > oldScore) {
                            const scoreContainer = document.getElementById('scoreContainer');
                            scoreContainer.classList.add('score-increased');
                            setTimeout(() => {
                                scoreContainer.classList.remove('score-increased');
                            }, 200);
                        }
                        
                        renderGrid();
                        if (data.game_over) {
                            document.getElementById('gameOver').classList.add('show');
                        }
                    }
                });
            }
        });
        
        // Touch support for mobile
        let touchStartX = 0;
        let touchStartY = 0;
        
        document.addEventListener('touchstart', function(e) {
            touchStartX = e.changedTouches[0].screenX;
            touchStartY = e.changedTouches[0].screenY;
        });
        
        document.addEventListener('touchend', function(e) {
            const touchEndX = e.changedTouches[0].screenX;
            const touchEndY = e.changedTouches[0].screenY;
            
            const dx = touchEndX - touchStartX;
            const dy = touchEndY - touchStartY;
            
            let direction = null;
            
            if (Math.abs(dx) > Math.abs(dy)) {
                if (dx > 0) direction = 'right';
                else direction = 'left';
            } else {
                if (dy > 0) direction = 'down';
                else direction = 'up';
            }
            
            if (direction) {
                fetch('/move', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ direction: direction })
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
