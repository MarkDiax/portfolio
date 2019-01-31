#include "Menu.h"
#include "mge\config.hpp"


Menu::Menu(float width, float height)
{
	if (!menuMusic.openFromFile(config::MGE_AUDIO_PATH + "Menu/MenuMusic.wav")) {

		std::cout << "Couldnt load music." << std::endl;
	}
	menuMusic.setLoop(true);
	//menuMusic.play();

	if (!font.loadFromFile(config::MGE_FONT_PATH + "adler.ttf"))
	{
		std::cout << "Couldn't load Font" << std::endl;

	}
	if (!creditsTexture.loadFromFile(config::MGE_TEXTURE_PATH + "Menu/credits_names.png")) {
		std::cout << "couldn't load credits texture" << std::endl;
	}
	if (!menuTexture.loadFromFile(config::MGE_TEXTURE_PATH + "Menu/base_highres.png")) {
		std::cout << "couldn't load Menu texture" << std::endl;
	}
	menuSprite.setTexture(menuTexture);

	creditsSprite.setTexture(creditsTexture);
	creditsSprite.setOrigin(creditsTexture.getSize().x / 2, creditsTexture.getSize().y / 2);
	creditsSprite.setPosition(menuTexture.getSize().x / 8.5f, menuTexture.getSize().y / 1.75f);

	menuTexts[0].setFont(font);
	menuTexts[0].setCharacterSize(40);
	menuTexts[0].setColor(sf::Color::White);
	menuTexts[0].setStyle(sf::Text::Underlined);
	menuTexts[0].setString("Start");
	menuTexts[0].setPosition(sf::Vector2f(580, 530));
	menuTexts[0].setRotation(-19);
	menuTexts[0].setScale(1.3f, 1.3f);
	menuTexts[0].setOrigin(menuTexts[0].getLocalBounds().width / 2, (menuTexts[0].getLocalBounds().height / 2));


	menuTexts[1].setFont(font);
	menuTexts[1].setCharacterSize(32);
	menuTexts[1].setColor(sf::Color::Black);
	menuTexts[1].setString("Credits");
	menuTexts[1].setPosition(sf::Vector2f(615, 640));
	menuTexts[1].setRotation(-19);
	menuTexts[1].setOrigin(menuTexts[1].getLocalBounds().width / 2, (menuTexts[1].getLocalBounds().height / 2));

	menuTexts[2].setFont(font);
	menuTexts[2].setCharacterSize(32);
	menuTexts[2].setColor(sf::Color::Black);
	menuTexts[2].setString("Exit");
	menuTexts[2].setPosition(sf::Vector2f(655, 750));
	menuTexts[2].setRotation(-19);
	menuTexts[2].setOrigin(menuTexts[2].getLocalBounds().width / 2, (menuTexts[2].getLocalBounds().height / 2));

	selectedItemIndex = 0;
}


Menu::~Menu()
{
}

void Menu::draw(sf::RenderWindow &window)
{
	window.draw(menuSprite);
	for (int i = 0; i < MAX_NUMBER_OF_ITEMS; i++)
	{
		window.draw(menuTexts[i]);
	}
	if (creditsActivated) {
		window.draw(creditsSprite);
	}

	//copied from World.cpp:
	//this for loop cleans up the pointers for sound
	for (int i = 0; i < _soundVector.size(); i++) {
		if (_soundVector[i]->getStatus() == sf::SoundSource::Stopped) {
			_soundVector.erase(_soundVector.begin() + i);
			if (_soundVector.size() < 1) return;
		}
		else {
			return;
		}
	}
}

void Menu::PlayAudio(std::string pFilename) {
	//copied from World.cpp:
	//looks for stored soundBuffers and plays them
	//otherwise it loads a sound into the engine
	pFilename = config::MGE_AUDIO_PATH + pFilename;
	sf::Sound* sound = new sf::Sound;

	if (_soundMap.find(pFilename) != _soundMap.end()) {
		std::cout << "Soundfile found in cache: " + pFilename + ".. Returning" << std::endl;
		sf::SoundBuffer * buffer = _soundMap.find(pFilename)->second;
		for (int i = 0; i < _soundVector.size(); i++) {
			if (_soundVector[i]->getBuffer() == buffer) {
				//if the sound's already playing, cut it off and start from the beginning
				_soundVector[i]->stop();
			}
		}
		sound->setBuffer(*_soundMap[pFilename]);
		sound->play();
		_soundVector.push_back(sound);
	}
	else {
		sf::SoundBuffer * buffer = new sf::SoundBuffer;
		std::cout << "Filename: " + pFilename + " not found in cache. Importing sound.." << std::endl;
		if (!buffer->loadFromFile(pFilename)) std::cout << "Buffer wasn't able to load sound : " + pFilename << std::endl;
		sound->setBuffer(*buffer);
		sound->play();
		_soundMap.insert(std::make_pair(pFilename, buffer));
		_soundVector.push_back(sound);
	}
}

void Menu::MoveUp()
{
	PlayAudio("Menu/Click.wav");

	if (selectedItemIndex - 1 >= 0)
	{
		menuTexts[selectedItemIndex].setColor(sf::Color::Black);
		menuTexts[selectedItemIndex].setStyle(sf::Text::Regular);
		menuTexts[selectedItemIndex].setScale(1, 1);
		menuTexts[selectedItemIndex].setOrigin(menuTexts[selectedItemIndex].getLocalBounds().width / 2, (menuTexts[selectedItemIndex].getLocalBounds().height / 2));
		selectedItemIndex--;
		menuTexts[selectedItemIndex].setColor(sf::Color::White);
		menuTexts[selectedItemIndex].setStyle(sf::Text::Underlined);
		menuTexts[selectedItemIndex].setScale(1.3f, 1.3f);
		menuTexts[selectedItemIndex].setOrigin(menuTexts[selectedItemIndex].getLocalBounds().width / 2, (menuTexts[selectedItemIndex].getLocalBounds().height / 2));

	}
}

void Menu::MoveDown()
{
	PlayAudio("Menu/Click.wav");

	if (selectedItemIndex + 1 < MAX_NUMBER_OF_ITEMS)
	{
		menuTexts[selectedItemIndex].setColor(sf::Color::Black);
		menuTexts[selectedItemIndex].setStyle(sf::Text::Regular);
		menuTexts[selectedItemIndex].setScale(1, 1);
		menuTexts[selectedItemIndex].setOrigin(menuTexts[selectedItemIndex].getLocalBounds().width / 2, (menuTexts[selectedItemIndex].getLocalBounds().height / 2));
		selectedItemIndex++;
		menuTexts[selectedItemIndex].setColor(sf::Color::White);
		menuTexts[selectedItemIndex].setStyle(sf::Text::Underlined);
		menuTexts[selectedItemIndex].setScale(1.3f, 1.3f);
		menuTexts[selectedItemIndex].setOrigin(menuTexts[selectedItemIndex].getLocalBounds().width / 2, (menuTexts[selectedItemIndex].getLocalBounds().height / 2));
	}
}


void Menu::HighlightText(int index) {
	for (int i = 0; i < MAX_NUMBER_OF_ITEMS; i++)
	{
		if (i != index) {
			menuTexts[i].setColor(sf::Color::Black);
			menuTexts[i].setStyle(sf::Text::Regular);
			menuTexts[i].setScale(1, 1);
			menuTexts[i].setOrigin(menuTexts[i].getLocalBounds().width / 2, (menuTexts[i].getLocalBounds().height / 2));
		}
	}
	menuTexts[index].setColor(sf::Color::White);
	menuTexts[index].setStyle(sf::Text::Underlined);
	menuTexts[index].setScale(1.3f, 1.3f);
	menuTexts[index].setOrigin(menuTexts[index].getLocalBounds().width / 2, (menuTexts[index].getLocalBounds().height / 2));
}

void Menu::HighlightNone() {
	for (int i = 0; i < MAX_NUMBER_OF_ITEMS; i++)
	{
		menuTexts[i].setColor(sf::Color::Black);
		menuTexts[i].setStyle(sf::Text::Regular);
		menuTexts[i].setScale(1, 1);
		menuTexts[i].setOrigin(menuTexts[i].getLocalBounds().width / 2, (menuTexts[i].getLocalBounds().height / 2));
	}
}