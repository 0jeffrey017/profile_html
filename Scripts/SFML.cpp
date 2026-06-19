#include <SFML/Graphics.hpp>
#include <iostream>
#include <fstream>

/*
 * SFML Learning（担当：個人開発）
 *
 * 商業エンジンに頼らず、C++ と SFML だけでゲームの基礎を学ぶ学習プロジェクト。
 * 外部の config.txt から「ウィンドウ・フォント・図形」の設定を読み込み、
 * ランタイムで動的に図形を生成・管理する「データ駆動型」の構成を実践している。
 * 図形は sf::Transformable を継承した自作クラスで表現し、拡張しやすい設計とした。
 */

/// <summary>移動・反射する円。位置・速度・色・名前ラベルを自身で保持する。</summary>
class myCircle : public sf::Transformable {
    sf::CircleShape circle;
    std::string m_N;                 // 表示名
    float m_X, m_Y, m_SX, m_SY;      // 位置(X,Y) と 速度(SX,SY)
    int m_R, m_G, m_B;               // 色 (RGB)
    float m_Radius;                  // 半径
    sf::Text text;
public:
    myCircle(std::string N, float X, float Y, float SX, float SY, int R, int G, int B, float Radius)
        : m_N(N), m_X(X), m_Y(Y), m_SX(SX), m_SY(SY), m_R(R), m_G(G), m_B(B), m_Radius(Radius)
    {
        circle.setRadius(Radius);
        circle.setFillColor(sf::Color(R, G, B));
        circle.setPosition(X, Y);
    }

    sf::CircleShape getShape() const { return circle; }

    void setText(const sf::Font& font, int characterSize, const sf::Color& color) {
        text.setString(m_N);
        text.setFont(font);
        text.setCharacterSize(characterSize);
        text.setFillColor(color);
    }

    /// <summary>速度ぶん移動し、ウィンドウ端に達したら速度を反転して跳ね返す。</summary>
    void move(const int& windowWidth, const int& windowHeight) {
        circle.move(m_SX, m_SY);
        sf::Vector2f pos = circle.getPosition();

        // 左右の壁との衝突：はみ出しを端へスナップしてから速度を反転
        if (pos.x <= 0) {
            m_SX *= -1.0f;
            circle.setPosition(0, pos.y);
        } else if (pos.x + (m_Radius * 2) >= windowWidth) {
            m_SX *= -1.0f;
            circle.setPosition(windowWidth - (m_Radius * 2), pos.y);
        }

        // 上下の壁との衝突
        pos = circle.getPosition();
        if (pos.y <= 0) {
            m_SY *= -1.0f;
            circle.setPosition(pos.x, 0);
        } else if (pos.y + (m_Radius * 2) >= windowHeight) {
            m_SY *= -1.0f;
            circle.setPosition(pos.x, windowHeight - (m_Radius * 2));
        }
    }

    /// <summary>名前ラベルを図形の中心に揃えて返す。</summary>
    sf::Text showName() {
        sf::FloatRect textRect = text.getLocalBounds();
        text.setOrigin(textRect.left + textRect.width / 2.0f, textRect.top + textRect.height / 2.0f);
        text.setPosition(circle.getPosition().x + m_Radius, circle.getPosition().y + m_Radius);
        return text;
    }
};

/// <summary>円と同じ振る舞いを持つ矩形版（幅・高さで境界判定する点が異なる）。</summary>
class myRectangle : public sf::Transformable {
    sf::RectangleShape rectangle;
    std::string m_N;
    float m_X, m_Y, m_SX, m_SY;
    int m_R, m_G, m_B;
    float m_W, m_H;                  // 幅・高さ
    sf::Text text;
public:
    myRectangle(std::string N, float X, float Y, float SX, float SY, int R, int G, int B, float W, float H)
        : m_N(N), m_X(X), m_Y(Y), m_SX(SX), m_SY(SY), m_R(R), m_G(G), m_B(B), m_W(W), m_H(H)
    {
        rectangle.setSize({W, H});
        rectangle.setFillColor(sf::Color(R, G, B));
        rectangle.setPosition(X, Y);
    }

    sf::RectangleShape getShape() const { return rectangle; }

    void setText(const sf::Font& font, int characterSize, const sf::Color& color) {
        text.setString(m_N);
        text.setFont(font);
        text.setCharacterSize(characterSize);
        text.setFillColor(color);
    }

    void move(const int& windowWidth, const int& windowHeight) {
        rectangle.move(m_SX, m_SY);
        sf::Vector2f pos = rectangle.getPosition();

        if (pos.x <= 0) {
            m_SX *= -1.0f;
            rectangle.setPosition(0, pos.y);
        } else if (pos.x + m_W >= windowWidth) {
            m_SX *= -1.0f;
            rectangle.setPosition(windowWidth - m_W, pos.y);
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

    sf::Text showName() {
        sf::FloatRect textRect = text.getLocalBounds();
        text.setOrigin(textRect.left + textRect.width / 2.0f, textRect.top + textRect.height / 2.0f);
        text.setPosition(rectangle.getPosition().x + (m_W / 2.0f), rectangle.getPosition().y + (m_H / 2.0f));
        return text;
    }
};

int main()
{
    // --- データ駆動：config.txt を1単語ずつ読み、キーワードに応じて生成 ---
    std::ifstream file("config.txt");
    sf::RenderWindow window;
    std::string word;
    std::vector<myCircle> circles;
    std::vector<myRectangle> rectangles;

    sf::Font font;
    int font_S, font_R, font_G, font_B;
    sf::Color fontColor;
    int windowWidth, windowHeight;

    while (file >> word) {
        if (word == "Window") {
            // ウィンドウサイズを設定ファイルから生成
            file >> windowWidth >> windowHeight;
            window.create(sf::VideoMode(windowWidth, windowHeight), "My Game");
        }
        else if (word == "Font") {
            std::string F;
            file >> F >> font_S >> font_R >> font_G >> font_B;
            if (!font.loadFromFile(F)) std::cout << "Cant Load Font";
            fontColor.r = font_R;
            fontColor.g = font_G;
            fontColor.b = font_B;
        }
        else if (word == "Circle") {
            // 1行のパラメータから円を1つ生成してリストへ追加
            std::string N; float X, Y, SX, SY; int R, G, B; float Radius;
            file >> N >> X >> Y >> SX >> SY >> R >> G >> B >> Radius;
            myCircle circle(N, X, Y, SX, SY, R, G, B, Radius);
            circle.setText(font, font_S, fontColor);
            circles.push_back(circle);
        }
        else if (word == "Rectangle") {
            std::string N; float X, Y, SX, SY; int R, G, B; float W, H;
            file >> N >> X >> Y >> SX >> SY >> R >> G >> B >> W >> H;
            myRectangle rectangle(N, X, Y, SX, SY, R, G, B, W, H);
            rectangle.setText(font, font_S, fontColor);
            rectangles.push_back(rectangle);
        }
    }

    // --- メインループ：全図形を移動＆描画 ---
    while (window.isOpen())
    {
        sf::Event event;
        while (window.pollEvent(event))
        {
            if (event.type == sf::Event::Closed)
                window.close();
        }

        window.clear();
        for (auto& c : circles) {
            c.move(windowWidth, windowHeight);
            window.draw(c.getShape());
            window.draw(c.showName());
        }
        for (auto& rt : rectangles) {
            rt.move(windowWidth, windowHeight);
            window.draw(rt.getShape());
            window.draw(rt.showName());
        }
        window.display();
    }

    file.close();
    return 0;
}
