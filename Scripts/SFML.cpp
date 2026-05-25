#include <SFML/Graphics.hpp>
#include <iostream>
#include <fstream>

class myCircle : public sf::Transformable{
    sf::CircleShape circle;
    std::string m_N;
    float m_X, m_Y, m_SX, m_SY;
    int m_R, m_G, m_B;
    float m_Radius;SFML
    sf::Text text;
public:
    myCircle(std::string N,float X, float Y, float SX, float SY,int R, int G, int B,float Radius)
    : m_N(N), m_X(X), m_Y(Y), m_SX(SX), m_SY(SY), m_R(R), m_G(G), m_B(B),m_Radius(Radius)
    {
        circle.setRadius(Radius);
        circle.setFillColor(sf::Color(R,G,B));
        circle.setPosition(X,Y);
    }
    sf::CircleShape getShape() const
    {
        return circle;
    }
    void setText(const sf::Font & font,int characterSize,const sf::Color & color){
        text.setString(m_N);
        text.setFont(font);
        text.setCharacterSize(characterSize);
        text.setFillColor(color);
    }
    void move(const int & windowWidth,const int & windowHeight) {
        circle.move(m_SX, m_SY);
        sf::Vector2f pos = circle.getPosition();

        if (pos.x <= 0) {
            m_SX *= -1.0f;
            circle.setPosition(0, pos.y); // Snap to edge
        } else if (pos.x + (m_Radius * 2) >= windowWidth) {
            m_SX *= -1.0f;
            circle.setPosition(windowWidth - (m_Radius * 2), pos.y); // Snap to edge
        }

        pos = circle.getPosition(); 
        if (pos.y <= 0) {
            m_SY *= -1.0f;
            circle.setPosition(pos.x, 0);
        } else if (pos.y + (m_Radius * 2) >= windowHeight) {
            m_SY *= -1.0f;
            circle.setPosition(pos.x, windowHeight - (m_Radius * 2));
        }
    }
    sf::Text showName(){
        sf::FloatRect textRect = text.getLocalBounds();
        text.setOrigin(textRect.left + textRect.width/2.0f, textRect.top  + textRect.height/2.0f);
        text.setPosition(circle.getPosition().x + m_Radius, circle.getPosition().y + m_Radius);
        return text;
    }
};

class myRectangle : public sf::Transformable{
    sf::RectangleShape rectangle;
    std::string m_N;
    float m_X, m_Y, m_SX, m_SY;
    int m_R, m_G, m_B;
    float m_W,m_H;
    sf::Text text;
public:
    myRectangle(std::string N,float X, float Y, float SX, float SY,int R, int G, int B,float W, float H)
    : m_N(N), m_X(X), m_Y(Y), m_SX(SX), m_SY(SY), m_R(R), m_G(G), m_B(B), m_W(W), m_H(H)
    {
        rectangle.setSize({W,H});
        rectangle.setFillColor(sf::Color(R,G,B));
        rectangle.setPosition(X,Y);
    }
    sf::RectangleShape getShape() const
    {
        return rectangle;
    }
    void setText(const sf::Font & font,int characterSize,const sf::Color & color){
        text.setString(m_N);
        text.setFont(font);
        text.setCharacterSize(characterSize);
        text.setFillColor(color);
    }
    void move(const int & windowWidth,const int & windowHeight) {
        rectangle.move(m_SX, m_SY);
        sf::Vector2f pos = rectangle.getPosition();

        if (pos.x <= 0) {
            m_SX *= -1.0f;
            rectangle.setPosition(0, pos.y); // Snap to edge
        } else if (pos.x + m_W >= windowWidth) {
            m_SX *= -1.0f;
            rectangle.setPosition(windowWidth - m_W, pos.y); // Snap to edge
        }

        pos = rectangle.getPosition(); 
        if (pos.y <= 0) {
            m_SY *= -1.0f;
            rectangle.setPosition(pos.x, 0);
        } else if (pos.y + m_H >= windowHeight) {
            m_SY *= -1.0f;
            rectangle.setPosition(pos.x, windowHeight - m_H);
        }
    }
    sf::Text showName(){
        sf::FloatRect textRect = text.getLocalBounds();
        text.setOrigin(textRect.left + textRect.width/2.0f, textRect.top  + textRect.height/2.0f);
        text.setPosition(rectangle.getPosition().x + (m_W/2.0f), rectangle.getPosition().y + (m_H/2.0f));
        return text;
    }
};

int main()
{
    std::ifstream file("config.txt");
    sf::RenderWindow window;
    std::string word;
    std::vector<myCircle> circles;
    std::vector<myRectangle> rectangles;

    sf::Font font;
    int font_S, font_R, font_G, font_B;
    sf::Color fontColor;

    int windowWidth,windowHeight;

    while (file >> word) {
    if (word == "Window") {
        file >> windowWidth >> windowHeight;
        window.create(sf::VideoMode(windowWidth, windowHeight), "My Game");
        } 
        else if (word == "Font") {
            std::string F;
            file >> F >> font_S >> font_R >> font_G >> font_B;
            if (!font.loadFromFile(F))
            {
                std::cout << "Cant Load Font";
            }
            fontColor.r = font_R;
            fontColor.g = font_G;
            fontColor.b = font_B;
        } 
        else if (word == "Circle") {
            std::string N;
            float X, Y, SX, SY;
            int R, G, B;
            float Radius;
            file >> N >> X >> Y >> SX >> SY >> R >> G >> B >> Radius;
            myCircle circle(N,X,Y,SX,SY,R,G,B,Radius);
            circle.setText(font,font_S,fontColor);
            circles.push_back(circle);
        }
        else if (word == "Rectangle") {
            std::string N;
            float X, Y, SX, SY;
            int R, G, B;
            float W, H;

            file >> N >> X >> Y >> SX >> SY >> R >> G >> B >> W >> H;
            myRectangle rectangle(N,X,Y,SX,SY,R,G,B,W,H);
            rectangle.setText(font,font_S,fontColor);

            rectangles.push_back(rectangle);
        }
        else {
            // Default case
        }
    }

    while (window.isOpen())
    {
        sf::Event event;
        while (window.pollEvent(event))
        {
            if (event.type == sf::Event::Closed)
                window.close();
        }

        window.clear();
        for(auto& c : circles){
            c.move(windowWidth,windowHeight);
            window.draw(c.getShape());
            window.draw(c.showName());
        }
        for(auto& rt : rectangles){
            rt.move(windowWidth,windowHeight);
            window.draw(rt.getShape());
            window.draw(rt.showName());
        }

        window.display();
    }

    file.close();
    return 0;
}