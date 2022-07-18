using UnityEngine;

namespace FightDemo {
    public class EnemyCollectiveController : MonoBehaviour {
        [System.Serializable]
        public class Enemy {
            public EnemyController controller;
            public Transform tr;

            public int number = -1;
            public bool current = false;
        }

        public Enemy[] enemies;

        public GameObject enemyPrefab;
        public int enemiesCount = 20;

        public Transform targetBody;
        public Transform targetHead;
        public HitController targetHitController;

        private int maxCurrentEnemies = 5;

        private float rotation;

        private float attackTimer;

        // Use this for initialization
        void Start() {
            targetHitController = targetBody.GetComponent<HitController>();

            enemies = new Enemy[enemiesCount];
            for (int i = 0; i < enemiesCount; i++) {

            }

            for (int i = 0; i < enemiesCount; i++) {
                float angle = (float)i / enemies.Length * 360;
                Quaternion rot = Quaternion.Euler(0, angle, 0);
                enemies[i] = new Enemy();
                GameObject enemy = Instantiate(enemyPrefab, targetBody.position + rot * Vector3.forward * 4, rot);
                enemies[i].controller = enemy.GetComponent<Redirector>().enemyController;
                enemies[i].controller.SetEnemyCollectiveController(this);
                enemies[i].controller.targetHead = targetHead;
                enemies[i].controller.targetBody = targetBody;
                enemies[i].tr = enemies[i].controller.transform;
            }
            NewEnemiesCircle();
        }

        public void NewEnemiesCircle() {
            maxCurrentEnemies = Mathf.Max(2, maxCurrentEnemies - 1);
            maxCurrentEnemies = Random.Range(maxCurrentEnemies, 7);
            rotation = Random.Range(0f, 360f);

            foreach (Enemy enemy in enemies) {
                enemy.current = false;
                enemy.number = -1;
                enemy.controller.isAttackState = false;
            }

            for (int a = 0; a < maxCurrentEnemies; a++) {
                float angle = ((float)a / maxCurrentEnemies) * 360;
                Quaternion rot = Quaternion.Euler(0, angle + rotation, 0);
                Vector3 pos = targetBody.position + rot * Vector3.forward * 2;

                int nearest = 0;
                for (int i = 0; i < enemies.Length; i++) {
                    if (enemies[i].number == -1 && !enemies[i].controller.IsDead) {
                        float dist = Vector3.Distance(enemies[i].tr.position, pos);
                        float distLast = Vector3.Distance(enemies[nearest].tr.position, pos);
                        if (dist <= distLast) {
                            nearest = i;
                        }
                    }
                }

                enemies[nearest].current = true;
                enemies[nearest].number = a;
            }

            for (int a = 0; a < (enemies.Length - maxCurrentEnemies); a++) {
                float angle = ((float)a / (enemies.Length - maxCurrentEnemies)) * 360;
                Quaternion rot = Quaternion.Euler(0, angle, 0);
                Vector3 pos = targetBody.position + rot * Vector3.forward * 5;

                int nearest = 0;
                for (int i = 0; i < enemies.Length; i++) {
                    if (enemies[i].number == -1 && !enemies[i].current) {
                        float dist = Vector3.Distance(enemies[i].tr.position, pos);
                        float distLast = Vector3.Distance(enemies[nearest].tr.position, pos);
                        if (dist <= distLast) {
                            nearest = i;
                        }
                    }
                }

                enemies[nearest].number = a;
            }

            foreach (Enemy enemy in enemies) {
                //Debug.Log(enemy.currentNumber);
            }
        }

        // Update is called once per frame
        void Update() {

            if (attackTimer > 0) {
                attackTimer -= Time.deltaTime;
            } else {
                if (!targetHitController.IsDead) {
                    int randomEnemy = Random.Range(0, maxCurrentEnemies);
                    foreach (Enemy enemy in enemies) {
                        if (enemy.number == randomEnemy && enemy.current) {
                            enemy.controller.isAttackState = true;
                            break;
                        }
                    }
                }
                attackTimer = Random.Range(0.2f, 2f);
            }

            int otherNumber = 0;
            foreach (Enemy enemy in enemies) {
                if (enemy.current) {
                    Vector3 target = Vector3.zero;
                    if (enemy.controller.isAttackState) {
                        target = targetBody.position;
                        enemy.controller.SetMoveTarget(target, true);
                    } else {
                        float angle = ((float)enemy.number / maxCurrentEnemies) * 360;
                        Quaternion rot = Quaternion.Euler(0, angle + rotation, 0);
                        target = targetBody.position + rot * Vector3.forward * 2;
                        enemy.controller.SetMoveTarget(target, false);
                    }
                } else {
                    float angle = ((float)enemy.number / (enemies.Length - maxCurrentEnemies)) * 360;
                    Quaternion rot = Quaternion.Euler(0, angle, 0);
                    enemy.controller.SetMoveTarget(targetBody.position + rot * Vector3.forward * 5, false);
                    otherNumber++;
                }
            }
        }
    }
}