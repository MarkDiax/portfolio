#pragma once
#include "SFML/Graphics.hpp"
#include "SFML/Audio.hpp"
#include <iostream>
#include <unordered_map>
#include <vector>

#define MAX_NUMBER_OF_ITEMS 3

class Menu
{
public:
	Menu(float width, float height);
	~Menu();

	void draw(sf::RenderWindow &window);
	void MoveUp();
	void MoveDown();
	void PlayAudio(std::string pFilename);
	int GetPressedItem() { return selectedItemIndex; }
	bool creditsActivated = false;

	void HighlightText(int index);
	void HighlightNone();
	void SelectText(int index);
	
	sf::Text menuTexts[MAX_NUMBER_OF_ITEMS];
	sf::Music menuMusic;

private:
	int selectedItemIndex;
	sf::Font font;
	
	sf::Sprite creditsSprite;
	sf::Texture creditsTexture;
	sf::Sprite menuSprite;
	sf::Texture menuTexture;

	sf::Sound menuSound;

	std::unordered_map <std::string, sf::SoundBuffer * > _soundMap;
	std::vector<sf::Sound*> _soundVector;
};

