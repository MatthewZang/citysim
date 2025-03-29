from flask import Flask, render_template, request, jsonify
import os
import json
from datetime import datetime

app = Flask(__name__)

# Replace this with your actual Mapbox token
MAPBOX_TOKEN = os.environ.get('MAPBOX_TOKEN', 'YOUR_MAPBOX_TOKEN')

SAVES_DIR = 'saves'
os.makedirs(SAVES_DIR, exist_ok=True)

# Game state
game_state = {
    'budget': 1000000,
    'population': 1000,
    'happiness': 50,
    'day': 1,
    'time': 0,
    'timeScale': 1,
    'buildings': [],
    'services': {
        'police': 0,
        'fire': 0,
        'education': 0,
        'healthcare': 0
    }
}

# Save game data
def save_game(city_name):
    save_data = {
        'city_name': city_name,
        'game_state': game_state,
        'timestamp': datetime.now().isoformat()
    }
    with open(os.path.join(SAVES_DIR, f'{city_name}_{datetime.now().strftime("%Y%m%d_%H%M%S")}.json'), 'w') as f:
        json.dump(save_data, f)

# Load game data
def load_game(city_name):
    try:
        with open(os.path.join(SAVES_DIR, f'{city_name}_{datetime.now().strftime("%Y%m%d_%H%M%S")}.json'), 'r') as f:
            save_data = json.load(f)
            global game_state
            game_state = save_data['game_state']
            return True
    except:
        return False

@app.route('/')
def index():
    return render_template('index.html', mapbox_token=MAPBOX_TOKEN)

@app.route('/api/game/state', methods=['GET'])
def get_game_state():
    return jsonify(game_state)

@app.route('/api/game/update', methods=['POST'])
def update_game():
    data = request.json
    game_state.update(data)
    return jsonify({'status': 'success'})

@app.route('/api/game/save', methods=['POST'])
def save():
    data = request.json
    city_name = data.get('city_name')
    if city_name:
        save_game(city_name)
        return jsonify({'status': 'success'})
    return jsonify({'status': 'error', 'message': 'City name required'})

@app.route('/api/game/load', methods=['POST'])
def load():
    data = request.json
    city_name = data.get('city_name')
    if city_name and load_game(city_name):
        return jsonify({'status': 'success', 'game_state': game_state})
    return jsonify({'status': 'error', 'message': 'Failed to load game'})

@app.route('/save', methods=['POST'])
def save_game():
    try:
        save_data = request.json
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        city_name = save_data.get('cityName', 'unnamed_city')
        filename = f"{city_name}_{timestamp}.json"
        
        with open(os.path.join(SAVES_DIR, filename), 'w') as f:
            json.dump(save_data, f)
        
        return jsonify({'status': 'success', 'filename': filename})
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/saves', methods=['GET'])
def list_saves():
    try:
        saves = []
        for filename in os.listdir(SAVES_DIR):
            if filename.endswith('.json'):
                with open(os.path.join(SAVES_DIR, filename), 'r') as f:
                    save_data = json.load(f)
                    saves.append(save_data)
        return jsonify(saves)
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True) 