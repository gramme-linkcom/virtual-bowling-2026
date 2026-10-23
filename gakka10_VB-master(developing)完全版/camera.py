import cv2
import sys
import time
import numpy as np
from collections import deque
from ball import Ball
from motion import Motion
from constant import CAMERA_FPS
from constant import CAMERA_WIDTH
from constant import CAMERA_HEIGHT



class NormalCamera:
    CV_WAITKEY_ENTER = 13
    CV_WAITKEY_ESC = 27
    CV_WAITKEY_R = 114

    def getCode(self, value):
        return 1 if value >= 0 else -1

    def __init__(self, camera_id): #camera_id
        self.capture = cv2.VideoCapture(camera_id,cv2.CAP_DSHOW)
        self.capture.set(cv2.CAP_PROP_FRAME_WIDTH, CAMERA_WIDTH)
        self.capture.set(cv2.CAP_PROP_FRAME_HEIGHT, CAMERA_HEIGHT)
        self.capture.set(cv2.CAP_PROP_FPS, CAMERA_FPS)

        self.mog = cv2.createBackgroundSubtractorMOG2(
            history = 100,
            varThreshold = 16,
            detectShadows = False
        )

    def detectBallProperty(self):
        # Enhanced detection:
        # - Use CLAHE to normalize lighting
        # - Gaussian blur + Canny for edges
        # - HoughCircles with tuned parameters
        # - Fallback to contour-based minEnclosingCircle
        # - Temporal smoothing (median over several frames) to improve stability
        FRAME_BUFFER = 6
        radii_buffer = []
        while True:
            circles = None
            ret, frame = self.capture.read()
            if not ret or frame is None:
                # wait a bit and continue
                time.sleep(0.01)
                continue

            # grayscale and CLAHE (improve contrast under uneven lighting)
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
            equal = clahe.apply(gray)
            gauss = cv2.GaussianBlur(equal, (7, 7), 0)

            # Try HoughCircles first with more forgiving thresholds
            result = cv2.HoughCircles(image=gauss, method=cv2.HOUGH_GRADIENT,
                                      dp=1.2, param1=100, param2=50,
                                      minDist=50, minRadius=20, maxRadius=400)

            detected = False
            if result is not None and len(result) > 0:
                # Choose the best (largest) circle candidate
                circles = result[0]
                best = max(circles, key=lambda c: c[2])
                x, y, r = map(int, best)
                cv2.circle(frame, (int(x), int(y)), int(r), (0, 255, 0), 2)
                radii_buffer.append(r)
                detected = True
            else:
                # Fallback: threshold + morphology + contours
                _, th = cv2.threshold(gauss, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
                kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
                th = cv2.morphologyEx(th, cv2.MORPH_OPEN, kernel)
                contours, _ = cv2.findContours(th, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
                best_r = 0
                best_xy = None
                for cnt in contours:
                    area = cv2.contourArea(cnt)
                    if area < 200:  # skip very small noise
                        continue
                    (cx, cy), radius = cv2.minEnclosingCircle(cnt)
                    if radius > best_r:
                        best_r = radius
                        best_xy = (int(cx), int(cy))
                if best_r > 5 and best_xy is not None:
                    x, y, r = best_xy[0], best_xy[1], int(best_r)
                    cv2.circle(frame, (int(x), int(y)), int(r), (255, 0, 0), 2)
                    radii_buffer.append(r)
                    detected = True

            # keep buffer size bounded
            if len(radii_buffer) > FRAME_BUFFER:
                radii_buffer.pop(0)

            cv2.imshow('Detecting Ball Property...', frame)

            key = cv2.waitKey(1)
            if key == self.CV_WAITKEY_ESC:
                self.capture.release()
                cv2.destroyAllWindows()
                sys.exit()
            # When user presses Enter and we have a stable estimate, return median radius
            elif key == self.CV_WAITKEY_ENTER and len(radii_buffer) >= FRAME_BUFFER:
                cv2.destroyAllWindows()
                median_r = int(np.median(radii_buffer))
                print('[System] 推定された円の半径（中央値）: ' + str(median_r))
                # 計測確定：クールダウンを設定してから返す
                self._last_measure_time = time.perf_counter()
                return Ball(median_r)

    def detectBallMotion(self, ball, waitTime = 0.2, resolution = 32):
        # Enter 押下で検出を開始するよう待機するループ
        print("[System] Enterキーでモーションの検出を開始します")
        while True:
            ret, frame = self.capture.read()
            if not ret or frame is None:
                time.sleep(0.01)
                continue
            # 画面に開始メッセージを表示して待つ
            display = frame.copy()
            cv2.putText(display, "Enter before detecting ball motion", (20, 40), cv2.FONT_HERSHEY_SIMPLEX, 1.0, (0,255,0), 2)
            cv2.imshow('Detecting Ball Motion...', display)
            key = cv2.waitKey(1)
            if key == self.CV_WAITKEY_ENTER:
                print('[System] モーションの検出を開始します')
                break
            elif key == self.CV_WAITKEY_ESC:
                self.capture.release()
                cv2.destroyAllWindows()
                sys.exit()

        """
        ボールの運動を検出して Motion を返す。

        - 背景差分マスクにモルフォロジー処理を入れてノイズを減らす
        - 輪郭 -> minEnclosingCircle で候補を評価
        - 候補の位置をバッファ（deque）に保存し、十分なフレームが溜まったら速度を算出
        - 日本語コメントを追加して可読性を向上
        """

        # 初期化
        start_time = None
        pos_buffer = deque(maxlen=32)  # (timestamp, row, col)
        none_detected_frames = 0
        MIN_AREA = 150  # ノイズ除去のための最小輪郭面積
        STABLE_FRAMES = max(4, int(CAMERA_FPS * waitTime))

        while True:
            ret, frame = self.capture.read()
            if not ret or frame is None:
                # フレーム取得に失敗したら少し待つ
                time.sleep(0.01)
                continue

            # 背景差分を取得し、モルフォロジーで開閉処理を行う（ノイズ除去）
            fgmask = self.mog.apply(frame)
            kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
            fgmask = cv2.morphologyEx(fgmask, cv2.MORPH_OPEN, kernel)
            fgmask = cv2.morphologyEx(fgmask, cv2.MORPH_CLOSE, kernel)

            # 輪郭抽出
            contours, _ = cv2.findContours(fgmask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

            frame_h, frame_w = fgmask.shape
            best_candidate = None
            best_area = 0
            # 各輪郭について最小外接円を求め、面積が最大のものをボール候補とする
            for cnt in contours:
                area = cv2.contourArea(cnt)
                if area < MIN_AREA:
                    continue
                (cx, cy), radius = cv2.minEnclosingCircle(cnt)
                if (( radius <= radius - 5 ) or ( radius >= radius + 5 )):
                    continue
                if area > best_area:
                    best_area = area
                    best_candidate = (int(cx), int(cy), int(radius))

            # デバッグ用にマスクを合成表示
            masked = cv2.bitwise_and(frame, frame, mask=fgmask)
            if best_candidate is not None:
                cx, cy, r = best_candidate
                cv2.circle(masked, (cx, cy), r, (0, 255, 0), 2)
                timestamp = time.perf_counter()
                pos_buffer.append((timestamp, cy, cx))
                if start_time is None:
                    start_time = timestamp
                none_detected_frames = 0
            else:
                none_detected_frames += 1

            cv2.imshow('Detecting Ball Motion...', masked)

            # 十分な検出データが貯まったら速度を算出して返す
            if len(pos_buffer) >= STABLE_FRAMES and none_detected_frames >= int(CAMERA_FPS * 0.05):
                # バッファから時刻・座標配列を作成
                times = np.array([p[0] for p in pos_buffer])
                ys = np.array([p[1] for p in pos_buffer])
                xs = np.array([p[2] for p in pos_buffer])

                # 線形回帰（最小二乗）で vx, vy を推定（時刻 vs 座標）
                t_mean = times.mean()
                dt_arr = times - t_mean
                dt_var = (dt_arr * dt_arr).sum()
                if dt_var < 1e-8:
                    print('[Debug] dt variance too small, retrying')
                else:
                    # 回帰係数（傾き） = cov(t,x) / var(t)
                    vx_reg = ((dt_arr * (xs - xs.mean())).sum()) / dt_var
                    vy_reg = ((dt_arr * (ys - ys.mean())).sum()) / dt_var

                    # 連続ペアの瞬時速度も計算して中央値をとる（速い動きを捕まえる目的）
                    inst_vx = []
                    inst_vy = []
                    for i in range(1, len(times)):
                        dt_pair = times[i] - times[i-1]
                        if dt_pair <= 1e-4:
                            continue
                        dx = xs[i] - xs[i-1]
                        dy = ys[i] - ys[i-1]
                        inst_vx.append(dx / dt_pair)
                        inst_vy.append(dy / dt_pair)

                    median_vx = float(np.median(inst_vx)) if len(inst_vx) > 0 else vx_reg
                    median_vy = float(np.median(inst_vy)) if len(inst_vy) > 0 else vy_reg

                    # 最終的な速度は回帰と瞬時速度のうち、絶対値の大きい方を採用して
                    # 速い動きが過小評価されるケースを防ぐ
                    vx = vx_reg
                    vy = vy_reg
                    if abs(median_vx) > abs(vx_reg) * 1.05:
                        vx = median_vx
                    if abs(median_vy) > abs(vy_reg) * 1.05:
                        vy = median_vy

                    firstPosition = xs[0] - (frame_w / 2)
                    # if firstPosition > 0:
                    #     print('[Debug] 初期位置が不適切です。再度投球してください。')
                    #     pos_buffer.clear()
                    #     start_time = None
                    #     none_detected_frames = 0
                    #     cv2.destroyAllWindows()
                    #     time.sleep(1.0)
                    #     return -1
                    # y方向の向きを補正（画面座標系に依存するため環境で要調整）
                    direction_y = -1 if vy > 0 else 1
                    velocity = (vx, vy * direction_y)
                    print(f'[Debug] vx_reg={vx_reg:.2f}, vy_reg={vy_reg:.2f}, median_vx={median_vx:.2f}, median_vy={median_vy:.2f}')
                    print(f'[Debug] chosen vx={vx:.2f}, vy={vy:.2f}, firstPosition={firstPosition:.1f}')
                    # 計測確定：クールダウンを設定してから返す
                    self._last_measure_time = time.perf_counter()
                    return Motion(firstPosition, velocity)

            key = cv2.waitKey(1)
            if key == self.CV_WAITKEY_ESC:
                self.capture.release()
                cv2.destroyAllWindows()
                sys.exit()
            elif key == self.CV_WAITKEY_R:
                # リセット: バッファと状態を初期化
                print('[System] R キーが押されました。計測結果をリセットします')
                pos_buffer.clear()
                start_time = None
                none_detected_frames = 0
                cv2.destroyAllWindows()
                return -1


class StereoCamera:
    def __init__(self, camera_id):
        print('Write later...')

    def detectBallProperty(self, color):
        print('Write later...')

    def detectBallMotion(self, ball):
        print('Write later...')
