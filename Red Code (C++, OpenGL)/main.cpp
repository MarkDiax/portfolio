#include <iostream>
#include "SFML/Graphics.hpp"
#include "Menu.h"
#include "mge/core/AbstractGame.hpp"
#include "mge/MGEDemo.hpp"
#include "MouseManager.h"


bool quitGame = false;		//whether the player has quit the game

void startGame() {
	std::cout << "Starting Game" << std::endl;

	AbstractGame* game = new MGEDemo();
	game->initialize();
	game->run();

	delete game;
}

void startMenu() {
	std::cout << "Starting Menu" << std::endl;

	sf::RenderWindow * window = new sf::RenderWindow(sf::VideoMode(1920, 1080), "RedCode - Main Menu", sf::Style::Fullscreen);
	window->setVerticalSyncEnabled(true);

	Menu * menu = new Menu(window->getSize().x, window->getSize().y);

	while (window->isOpen())
	{
		sf::Event event;
		while (window->pollEvent(event))
		{
			for (int i = 0; i < MAX_NUMBER_OF_ITEMS; i++) {
				if (menu->menuTexts[i].getGlobalBounds().contains((sf::Vector2f)sf::Mouse::getPosition(*window))) {
					menu->HighlightText(i);

					if (MouseManager::instance()->onMouseDown(sf::Mouse::Left)) 
					{
						switch (i) {
						case 0:
							std::cout << "Play button has been pressed" << std::endl;
							menu->menuMusic.stop();
							window->close();
							return;
						case 1:
							std::cout << "Credits button has been pressed" << std::endl;
							if (menu->creditsActivated) {
								menu->creditsActivated = false;
								break;
							}
							menu->creditsActivated = true;
							break;
						case 2:
							menu->menuMusic.stop();
							window->close();
							quitGame = true;
							break;
						}
					}
				}
			}

			switch (event.type) {

			case sf::Event::KeyReleased:
				switch (event.key.code) {

				case sf::Keyboard::Up:
					menu->MoveUp();
					break;

				case sf::Keyboard::Down:
					menu->MoveDown();
					break;

				case sf::Keyboard::Return:
					menu->PlayAudio("Menu/Click.wav");

					switch (menu->GetPressedItem()) {
					case 0:
						std::cout << "Play button has been pressed" << std::endl;
						menu->menuMusic.stop();
						window->close();
						return;
					case 1:
						std::cout << "Credits button has been pressed" << std::endl;
						if (menu->creditsActivated) {
							menu->creditsActivated = false;
							break;
						}
						menu->creditsActivated = true;
						break;
					case 2:
						menu->menuMusic.stop();
						window->close();
						quitGame = true;
						break;
					}
					break;

				case sf::Keyboard::Escape:
					menu->menuMusic.stop();
					window->close();
					quitGame = true;
					return;
				}
				break;
			case sf::Event::Closed:
				window->close();
				break;
			}
			break;
		}

		window->clear();
		menu->draw(*window);
		window->display();
	}

	menu->menuMusic.stop();
	delete window;
	delete menu;
}

int main()
{
	//Simply comment the following line while in development:
	startMenu();

	if (!quitGame) {
		startGame();
	}
	return 0;
}